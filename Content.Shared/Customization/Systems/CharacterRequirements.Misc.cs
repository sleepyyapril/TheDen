// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Customization.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Customization.Systems;

/// <summary>
///     Requires the server to have a specific CVar value.
/// </summary>
[UsedImplicitly, Serializable, NetSerializable,]
public sealed partial class CVarRequirement : CharacterRequirement
{
    [DataField("cvar", required: true)]
    public string CVar;

    [DataField(required: true)]
    public string RequiredValue;

    public override bool IsValid(
        JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        IPrototype prototype,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager,
        out string? reason,
        int depth = 0,
        MindComponent? mind = null
    )
    {
        if (!configManager.IsCVarRegistered(CVar))
        {
            reason = null;
            return true;
        }

        const string color = "lightblue";
        var cvar = configManager.GetCVar(CVar);
        var isValid = cvar.ToString()! == RequiredValue;

        reason = Loc.GetString(
            "character-cvar-requirement",
            ("inverted", Inverted),
            ("color", color),
            ("cvar", CVar),
            ("value", RequiredValue));

        return isValid;
    }
}

/// <summary>
///     Requires the server to have a player count within range.
/// </summary>
[UsedImplicitly, Serializable, NetSerializable]
public sealed partial class ServerPlayersRequirement : CharacterRequirement
{
    [DataField]
    public int MinimumPlayers = -1;

    [DataField]
    public int MaximumPlayers = int.MaxValue;

    [DataField]
    public bool OnlyCountPlayersInRound;

    public override bool IsValid(
        JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        IPrototype prototype,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager,
        out string? reason,
        int depth = 0,
        MindComponent? mind = null
    )
    {
        var playerManager = IoCManager.Resolve<ISharedPlayerManager>();
        var allPlayers = playerManager.Sessions;
        var playerCount = 0;

        foreach (var player in allPlayers)
        {
            if (!OnlyCountPlayersInRound)
            {
                playerCount++;
                continue;
            }

            if (player.AttachedEntity is { Valid: true })
                playerCount++;
        }

        var passed = playerCount > MinimumPlayers && playerCount < MaximumPlayers;
        var reasonString = Loc.GetString(
            "character-players-requirement",
            ("inverted", Inverted),
            ("min", MinimumPlayers),
            ("max", MaximumPlayers));

        reason = !passed ? reasonString : null;
        return passed;
    }
}
