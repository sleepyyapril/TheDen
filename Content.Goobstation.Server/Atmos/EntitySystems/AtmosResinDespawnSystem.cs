// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Spawners;
using Content.Shared.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Goobstation.Server.Atmos.Components;

namespace Content.Goobstation.Server.Atmos.EntitySystems;


/// <summary>
/// Assmos - Extinguisher Nozzle
/// Sets atmospheric temperature to 20C and removes all toxins. 
/// </summary>
public sealed class AtmosResinDespawnSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmo = default!;
    [Dependency] private readonly GasTileOverlaySystem _gasOverlaySystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AtmosResinDespawnComponent, TimedDespawnEvent>(OnDespawn);
    }

    private void OnDespawn(EntityUid uid, AtmosResinDespawnComponent comp, ref TimedDespawnEvent args)
    {
        if (!TryComp(uid, out TransformComponent? xform))
            return;

        var mix = _atmo.GetContainingMixture(uid, true);
        GasMixture newMix = new();

        if (mix is null) return;
        mix.AdjustMoles(Gas.CarbonDioxide, -mix.GetMoles(Gas.CarbonDioxide));
        mix.AdjustMoles(Gas.Plasma, -mix.GetMoles(Gas.Plasma));
        mix.AdjustMoles(Gas.Tritium, -mix.GetMoles(Gas.Tritium));
        mix.AdjustMoles(Gas.Ammonia, -mix.GetMoles(Gas.Ammonia));
        mix.AdjustMoles(Gas.NitrousOxide, -mix.GetMoles(Gas.NitrousOxide));
        mix.AdjustMoles(Gas.Frezon, -mix.GetMoles(Gas.Frezon));
        mix.AdjustMoles(Gas.BZ, -mix.GetMoles(Gas.BZ));
        mix.AdjustMoles(Gas.Healium, -mix.GetMoles(Gas.Healium));
        mix.AdjustMoles(Gas.Nitrium, -mix.GetMoles(Gas.Nitrium));
        mix.AdjustMoles(Gas.Hydrogen, -mix.GetMoles(Gas.Hydrogen));
        mix.AdjustMoles(Gas.HyperNoblium, -mix.GetMoles(Gas.HyperNoblium));
        mix.AdjustMoles(Gas.ProtoNitrate, -mix.GetMoles(Gas.ProtoNitrate));
        mix.AdjustMoles(Gas.Zauker, -mix.GetMoles(Gas.Zauker));
        mix.AdjustMoles(Gas.Halon, -mix.GetMoles(Gas.Halon));
        mix.AdjustMoles(Gas.Helium, -mix.GetMoles(Gas.Helium));
        mix.AdjustMoles(Gas.AntiNoblium, -mix.GetMoles(Gas.AntiNoblium));
        mix.Temperature = Atmospherics.T20C;
        _gasOverlaySystem.UpdateSessions();
    }
}
