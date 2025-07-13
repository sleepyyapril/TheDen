// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <flyingkarii@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Power.Components;

[NetworkedComponent]
public abstract partial class SharedApcPowerReceiverComponent : Component
{
    [ViewVariables]
    public bool Powered;

    /// <summary>
    ///     When false, causes this to appear powered even if not receiving power from an Apc.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual bool NeedsPower { get; set;}

    /// <summary>
    ///     When true, causes this to never appear powered.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public virtual bool PowerDisabled { get; set; }
}
