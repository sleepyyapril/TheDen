// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Diagnostics.CodeAnalysis;
using Content.Client.Power.Components;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Power;
using Robust.Shared.GameStates;

namespace Content.Client.Power.EntitySystems;

public sealed class PowerReceiverSystem : SharedPowerReceiverSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ApcPowerReceiverComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<ApcPowerReceiverComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnExamined(Entity<ApcPowerReceiverComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(GetExamineText(ent.Comp.Powered));
    }

    private void OnHandleState(EntityUid uid, ApcPowerReceiverComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not ApcPowerReceiverComponentState state)
            return;

        var powerChanged = component.Powered != state.Powered;
        component.Powered = state.Powered;
        component.NeedsPower = state.NeedsPower;
        component.PowerDisabled = state.PowerDisabled;
        // SO client systems can handle it. The main reason for this is we can't guarantee compstate ordering.

        if (powerChanged)
            RaisePower((uid, component));
    }

    protected override void RaisePower(Entity<SharedApcPowerReceiverComponent> entity)
    {
        var ev = new PowerChangedEvent(entity.Comp.Powered, 0f);
        RaiseLocalEvent(entity.Owner, ref ev);
    }

    public override bool ResolveApc(EntityUid entity, [NotNullWhen(true)] ref SharedApcPowerReceiverComponent? component)
    {
        if (component != null)
            return true;

        if (!TryComp(entity, out ApcPowerReceiverComponent? receiver))
            return false;

        component = receiver;
        return true;
    }
}
