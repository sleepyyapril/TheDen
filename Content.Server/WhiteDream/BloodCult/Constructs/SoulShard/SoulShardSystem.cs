// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Bible.Components;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Shared.Interaction;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.WhiteDream.BloodCult;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Constructs.SoulShard;

public sealed class SoulShardSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedPointLightSystem _lightSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SoulShardComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SoulShardComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<SoulShardComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<SoulShardComponent, MindAddedMessage>(OnShardMindAdded);
        SubscribeLocalEvent<SoulShardComponent, MindRemovedMessage>(OnShardMindRemoved);
    }

    private void OnMapInit(Entity<SoulShardComponent> shard, ref MapInitEvent args)
    {
        if (!shard.Comp.IsBlessed)
            return;

        _appearanceSystem.SetData(shard, SoulShardVisualState.Blessed, true);
        _lightSystem.SetColor(shard, shard.Comp.BlessedLightColor);
    }

    private void OnActivate(Entity<SoulShardComponent> shard, ref ActivateInWorldEvent args)
    {
        if (!_mind.TryGetMind(shard, out var mindId, out _))
            return;

        if (!shard.Comp.IsBlessed)
        {
            if (!HasComp<BloodCultistComponent>(args.User))
                return;
            if (shard.Comp.ShadeUid.HasValue)
                DespawnShade(shard);
            else
                SpawnShade(shard, shard.Comp.ShadeProto, mindId);
            return;
        }

        if (shard.Comp.ShadeUid.HasValue)
            DespawnShade(shard);
        else
            SpawnShade(shard, shard.Comp.PurifiedShadeProto, mindId);
    }

    private void OnInteractUsing(Entity<SoulShardComponent> shard, ref InteractUsingEvent args)
    {
        if (shard.Comp.IsBlessed || !TryComp(args.Used, out BibleComponent? bible))
            return;

        _popup.PopupEntity(Loc.GetString("bible-sizzle"), args.User, args.User);
        _audio.PlayPvs(bible.HealSoundPath, args.User);
        _appearanceSystem.SetData(shard, SoulShardVisualState.Blessed, true);
        _lightSystem.SetColor(shard, shard.Comp.BlessedLightColor);
        shard.Comp.IsBlessed = true;
    }

    private void OnShardMindAdded(Entity<SoulShardComponent> shard, ref MindAddedMessage args)
    {
        if (!TryComp<MindContainerComponent>(shard, out var mindContainer) || !mindContainer.HasMind)
            return;

        _roleSystem.MindTryRemoveRole<TraitorRoleComponent>(mindContainer.Mind.Value);
        UpdateGlowVisuals(shard, true);
    }

    private void OnShardMindRemoved(Entity<SoulShardComponent> shard, ref MindRemovedMessage args) =>
        UpdateGlowVisuals(shard, false);

    private void SpawnShade(Entity<SoulShardComponent> shard, EntProtoId proto, EntityUid mindId)
    {
        var position = _transform.GetMapCoordinates(shard);
        var shadeUid = Spawn(proto, position);
        _mind.TransferTo(mindId, shadeUid);
        _mind.UnVisit(mindId);
    }

    private void DespawnShade(Entity<SoulShardComponent> shard)
    {
        if (!_mind.TryGetMind(shard.Comp.ShadeUid!.Value, out var mindId, out _))
        {
            _mind.TransferTo(mindId, shard);
            _mind.UnVisit(mindId);
        }

        Del(shard.Comp.ShadeUid);
    }

    private void UpdateGlowVisuals(Entity<SoulShardComponent> shard, bool state)
    {
        _appearanceSystem.SetData(shard, SoulShardVisualState.HasMind, state);
        _lightSystem.SetEnabled(shard, state);
    }
}
