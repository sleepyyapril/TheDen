// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Speech.EntitySystems;

namespace Content.Server.Speech.Components;

[RegisterComponent]
[Access(typeof(PirateAccentSystem))]
public sealed partial class PirateAccentComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("yarrChance")]
    public float YarrChance = 0.5f;

    [ViewVariables]
    public readonly List<string> PirateWords = new()
    {
        "accent-pirate-prefix-1",
        "accent-pirate-prefix-2",
        "accent-pirate-prefix-3",
        "accent-pirate-prefix-4",
    };
}
