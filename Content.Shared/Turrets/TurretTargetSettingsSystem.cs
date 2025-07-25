// SPDX-FileCopyrightText: 2025 Solaris <60526456+SolarisBirb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Access;
using Content.Shared.Access.Systems;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared.Turrets;

/// <summary>
/// This system is used for validating potential targets for NPCs with a <see cref="TurretTargetSettingsComponent"/> (i.e., turrets).
/// A turret will consider an entity a valid target if the entity does not possess any access tags which appear on the
/// turret's <see cref="TurretTargetSettingsComponent.ExemptAccessLevels"/> list.
/// </summary>
public sealed partial class TurretTargetSettingsSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;

    private ProtoId<AccessLevelPrototype> _accessLevelBorg = "Borg";
    private ProtoId<AccessLevelPrototype> _accessLevelBasicSilicon = "BasicSilicon";

    /// <summary>
    /// Adds or removes access levels from a <see cref="TurretTargetSettingsComponent.ExemptAccessLevels"/> list.
    /// </summary>
    /// <param name="ent">The entity and its <see cref="TurretTargetSettingsComponent"/></param>
    /// <param name="exemption">The proto ID for the access level</param>
    /// <param name="enabled">Set 'true' to add the exemption, or 'false' to remove it</param>
    [PublicAPI]
    public void SetAccessLevelExemption(TurretTargetSettingsComponent comp, ProtoId<AccessLevelPrototype> exemption, bool enabled)
    {
        if (enabled)
            comp.ExemptAccessLevels.Add(exemption);
        else
            comp.ExemptAccessLevels.Remove(exemption);
    }

    /// <summary>
    /// Adds or removes a collection of access levels from a <see cref="TurretTargetSettingsComponent.ExemptAccessLevels"/> list.
    /// </summary>
    /// <param name="comp">The Component and its <see cref="TurretTargetSettingsComponent"/></param>
    /// <param name="exemption">The collection of access level proto IDs to add or remove</param>
    /// <param name="enabled">Set 'true' to add the collection as exemptions, or 'false' to remove them</param>
    [PublicAPI]
    public void SetAccessLevelExemptions(TurretTargetSettingsComponent comp, ICollection<ProtoId<AccessLevelPrototype>> exemptions, bool enabled)
    {
        foreach (var exemption in exemptions)
            SetAccessLevelExemption(comp, exemption, enabled);
    }

    /// <summary>
    /// Sets a <see cref="TurretTargetSettingsComponent.ExemptAccessLevels"/> list to contain only a supplied collection of access levels.
    /// </summary>
    /// <param name="comp">The component and its <see cref="TurretTargetSettingsComponent"/></param>
    /// <param name="exemptions">The supplied collection of access level proto IDs</param>
    [PublicAPI]
    public void SyncAccessLevelExemptions(TurretTargetSettingsComponent comp, ICollection<ProtoId<AccessLevelPrototype>> exemptions)
    {
        comp.ExemptAccessLevels.Clear();
        SetAccessLevelExemptions(comp, exemptions, true);
    }

    /// <summary>
    /// Sets a <see cref="TurretTargetSettingsComponent.ExemptAccessLevels"/> list to match that of another.
    /// </summary>
    /// <param name="target">The entity this is having its exemption list updated <see cref="TurretTargetSettingsComponent"/></param>
    /// <param name="source">The entity that is being used as a template for the target</param>
    [PublicAPI]
    public void SyncAccessLevelExemptions(Entity<TurretTargetSettingsComponent> target, Entity<TurretTargetSettingsComponent> source)
    {
        SyncAccessLevelExemptions(target.Comp, source.Comp.ExemptAccessLevels);
    }

    /// <summary>
    /// Returns whether a <see cref="TurretTargetSettingsComponent.ExemptAccessLevels"/> list contains a specific access level.
    /// </summary>
    /// <param name="ent">The entity and its <see cref="TurretTargetSettingsComponent"/></param>
    /// <param name="exemption">The access level proto ID being checked</param>
    [PublicAPI]
    public bool HasAccessLevelExemption(Entity<TurretTargetSettingsComponent> ent, ProtoId<AccessLevelPrototype> exemption)
    {
        if (ent.Comp.ExemptAccessLevels.Count == 0)
            return false;

        return ent.Comp.ExemptAccessLevels.Contains(exemption);
    }

    /// <summary>
    /// Returns whether a <see cref="TurretTargetSettingsComponent.ExemptAccessLevels"/> list contains one or more access levels from another collection.
    /// </summary>
    /// <param name="ent">The entity and its <see cref="TurretTargetSettingsComponent"/></param>
    /// <param name="exemptions"></param>
    [PublicAPI]
    public bool HasAnyAccessLevelExemption(Entity<TurretTargetSettingsComponent> ent, ICollection<ProtoId<AccessLevelPrototype>> exemptions)
    {
        if (ent.Comp.ExemptAccessLevels.Count == 0)
            return false;

        foreach (var exemption in exemptions)
        {
            if (HasAccessLevelExemption(ent, exemption))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns whether an entity is a valid target for a turret.
    /// </summary>
    /// <remarks>
    /// Returns false if the target possesses one or more access tags that are present on the entity's <see cref="TurretTargetSettingsComponent.ExemptAccessLevels"/> list.
    /// </remarks>
    /// <param name="ent">The entity and its <see cref="TurretTargetSettingsComponent"/></param>
    /// <param name="target">The target entity</param>
    [PublicAPI]
    public bool EntityIsTargetForTurret(Entity<TurretTargetSettingsComponent> ent, EntityUid target)
    {
        var accessLevels = _accessReader.FindAccessTags(target);

        if (accessLevels.Contains(_accessLevelBorg))
            return !HasAccessLevelExemption(ent, _accessLevelBorg);

        if (accessLevels.Contains(_accessLevelBasicSilicon))
            return !HasAccessLevelExemption(ent, _accessLevelBasicSilicon);

        return !HasAnyAccessLevelExemption(ent, accessLevels);
    }
}
