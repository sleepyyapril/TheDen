// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2024 SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Maps;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared.Weather;

public abstract class SharedWeatherSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly IMapManager MapManager = default!;
    [Dependency] protected readonly IPrototypeManager ProtoMan = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private EntityQuery<BlockWeatherComponent> _blockQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();
        _blockQuery = GetEntityQuery<BlockWeatherComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        SubscribeLocalEvent<WeatherComponent, EntityUnpausedEvent>(OnWeatherUnpaused);
    }

    private void OnWeatherUnpaused(EntityUid uid, WeatherComponent component, ref EntityUnpausedEvent args)
    {
        foreach (var weather in component.Weather.Values)
        {
            weather.StartTime += args.PausedTime;

            if (weather.EndTime != null)
                weather.EndTime = weather.EndTime.Value + args.PausedTime;
        }
    }

    public bool CanWeatherAffect(
        MapGridComponent grid,
        TileRef tileRef)
    {
        if (tileRef.Tile.IsEmpty)
            return true;

        var tileDef = (ContentTileDefinition) _tileDefManager[tileRef.Tile.TypeId];

        if (!tileDef.Weather)
            return false;

        var anchoredEnts = grid.GetAnchoredEntitiesEnumerator(tileRef.GridIndices);

        while (anchoredEnts.MoveNext(out var ent))
        {
            if (_blockQuery.HasComponent(ent.Value))
                return false;
        }

        return true;

    }

    public float GetPercent(WeatherData component, EntityUid mapUid)
    {
        var pauseTime = _metadata.GetPauseTime(mapUid);
        var elapsed = Timing.CurTime - (component.StartTime + pauseTime);
        var duration = component.Duration;
        var remaining = duration - elapsed;
        float alpha;

        if (remaining < WeatherComponent.ShutdownTime)
        {
            alpha = (float) (remaining / WeatherComponent.ShutdownTime);
        }
        else if (elapsed < WeatherComponent.StartupTime)
        {
            alpha = (float) (elapsed / WeatherComponent.StartupTime);
        }
        else
        {
            alpha = 1f;
        }

        return alpha;
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!Timing.IsFirstTimePredicted)
            return;

        var curTime = Timing.CurTime;

        var query = EntityQueryEnumerator<WeatherComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Weather.Count == 0)
                continue;

            foreach (var (proto, weather) in comp.Weather)
            {
                var endTime = weather.EndTime;

                // Ended
                if (endTime != null && endTime < curTime)
                {
                    EndWeather(uid, comp, proto);
                    continue;
                }

                var remainingTime = endTime - curTime;

                // Admin messed up or the likes.
                if (!ProtoMan.TryIndex<WeatherPrototype>(proto, out var weatherProto))
                {
                    Log.Error($"Unable to find weather prototype for {comp.Weather}, ending!");
                    EndWeather(uid, comp, proto);
                    continue;
                }

                // Shutting down
                if (endTime != null && remainingTime < WeatherComponent.ShutdownTime)
                {
                    SetState(uid, WeatherState.Ending, comp, weather, weatherProto);
                }
                // Starting up
                else
                {
                    var startTime = weather.StartTime;
                    var elapsed = Timing.CurTime - startTime;

                    if (elapsed < WeatherComponent.StartupTime)
                    {
                        SetState(uid, WeatherState.Starting, comp, weather, weatherProto);
                    }
                }

                // Run whatever code we need.
                Run(uid, weather, weatherProto, frameTime);
            }
        }
    }

    /// <summary>
    /// Shuts down all existing weather and starts the new one if applicable.
    /// </summary>
    public void SetWeather(MapId mapId, WeatherPrototype? proto, TimeSpan? endTime)
    {
        var mapUid = MapManager.GetMapEntityId(mapId);
        var weatherComp = EnsureComp<WeatherComponent>(mapUid);

        foreach (var (eProto, weather) in weatherComp.Weather)
        {
            // Reset cooldown if it's an existing one.
            if (eProto == proto?.ID)
            {
                weather.EndTime = endTime;

                if (weather.State == WeatherState.Ending)
                    weather.State = WeatherState.Running;

                Dirty(mapUid, weatherComp);
                continue;
            }

            // Speedrun
            var end = Timing.CurTime + WeatherComponent.ShutdownTime;

            if (weather.EndTime == null || weather.EndTime > end)
            {
                weather.EndTime = end;
                Dirty(mapUid, weatherComp);
            }
        }

        if (proto != null)
            StartWeather(mapUid, weatherComp, proto, endTime);
    }

    /// <summary>
    /// Run every tick when the weather is running.
    /// </summary>
    protected virtual void Run(EntityUid uid, WeatherData weather, WeatherPrototype weatherProto, float frameTime) { }

    protected void StartWeather(EntityUid uid, WeatherComponent component, WeatherPrototype weather, TimeSpan? endTime)
    {
        if (component.Weather.ContainsKey(weather.ID))
            return;

        var data = new WeatherData()
        {
            StartTime = Timing.CurTime,
            EndTime = endTime,
        };

        component.Weather.Add(weather.ID, data);
        Dirty(uid, component);
    }

    protected virtual void EndWeather(EntityUid uid, WeatherComponent component, string proto)
    {
        if (!component.Weather.TryGetValue(proto, out var data))
            return;

        _audio.Stop(data.Stream);
        data.Stream = null;
        component.Weather.Remove(proto);
        Dirty(uid, component);
    }

    protected virtual bool SetState(EntityUid uid, WeatherState state, WeatherComponent component, WeatherData weather, WeatherPrototype weatherProto)
    {
        if (weather.State.Equals(state))
            return false;

        weather.State = state;
        Dirty(uid, component);
        return true;
    }

    [Serializable, NetSerializable]
    protected sealed class WeatherComponentState : ComponentState
    {
        public Dictionary<string, WeatherData> Weather;

        public WeatherComponentState(Dictionary<string, WeatherData> weather)
        {
            Weather = weather;
        }
    }
}
