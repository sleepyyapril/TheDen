// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Snowni <101532866+Snowni@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.DeviceLinking.Systems;
using Content.Server.Explosion.Components;
using Content.Shared.DeviceLinking.Events;

namespace Content.Server.Explosion.EntitySystems
{
    public sealed partial class TriggerSystem
    {
        [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
        private void InitializeSignal()
        {
            SubscribeLocalEvent<TriggerOnSignalComponent, ComponentInit>(TriggerOnSignalInit);
            SubscribeLocalEvent<TriggerOnSignalComponent, SignalReceivedEvent>(OnSignalReceived);

            SubscribeLocalEvent<SignalOnTriggerComponent, ComponentInit>(SignalOnTriggerInit);
            SubscribeLocalEvent<SignalOnTriggerComponent, TriggerEvent>(HandleSignalOnTrigger);
        }
        
        private void TriggerOnSignalInit(EntityUid uid, TriggerOnSignalComponent component, ComponentInit args)
        {
            _signalSystem.EnsureSinkPorts(uid, component.Port);
        }

        private void OnSignalReceived(EntityUid uid, TriggerOnSignalComponent component, ref SignalReceivedEvent args)
        {
            if (args.Port != component.Port)
                return;

            Trigger(uid, args.Trigger);
        }
        
        // DENWIZ - rough import of wizden SignalOnTrigger
        private void SignalOnTriggerInit(EntityUid uid, SignalOnTriggerComponent component, ComponentInit args)
        {
            _signalSystem.EnsureSourcePorts(uid, component.Port);
        }

        private void HandleSignalOnTrigger(EntityUid uid, SignalOnTriggerComponent component, TriggerEvent args)
        {
            _signalSystem.InvokePort(uid, component.Port);
            args.Handled = true;
        }
    }
}
