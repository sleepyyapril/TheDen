// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Debug <49997488+DebugOk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Server.Power.Components;
using Content.Shared.Administration;
using Content.Shared.Construction;
using Content.Shared.Tag;
using Robust.Shared.Console;
using Robust.Shared.Map.Components;

namespace Content.Server.Construction.Commands
{
    [AdminCommand(AdminFlags.Mapping)]
    public sealed class FixRotationsCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entManager = default!;

        // ReSharper disable once StringLiteralTypo
        public string Command => "fixrotations";
        public string Description => "Sets the rotation of all occluders, low walls and windows to south.";
        public string Help => $"Usage: {Command} <gridId> | {Command}";

        public void Execute(IConsoleShell shell, string argsOther, string[] args)
        {
            var player = shell.Player;
            EntityUid? gridId;
            var xformQuery = _entManager.GetEntityQuery<TransformComponent>();

            switch (args.Length)
            {
                case 0:
                    if (player?.AttachedEntity is not {Valid: true} playerEntity)
                    {
                        shell.WriteError("Only a player can run this command.");
                        return;
                    }

                    gridId = xformQuery.GetComponent(playerEntity).GridUid;
                    break;
                case 1:
                    if (!NetEntity.TryParse(args[0], out var idNet) || !_entManager.TryGetEntity(idNet, out var id))
                    {
                        shell.WriteError($"{args[0]} is not a valid entity.");
                        return;
                    }

                    gridId = id;
                    break;
                default:
                    shell.WriteLine(Help);
                    return;
            }

            if (!_entManager.TryGetComponent(gridId, out MapGridComponent? grid))
            {
                shell.WriteError($"No grid exists with id {gridId}");
                return;
            }

            if (!_entManager.EntityExists(gridId))
            {
                shell.WriteError($"Grid {gridId} doesn't have an associated grid entity.");
                return;
            }

            var changed = 0;
            var tagSystem = _entManager.EntitySysManager.GetEntitySystem<TagSystem>();


            var enumerator = xformQuery.GetComponent(gridId.Value).ChildEnumerator;
            while (enumerator.MoveNext(out var child))
            {
                if (!_entManager.EntityExists(child))
                {
                    continue;
                }

                var valid = false;

                // Occluders should only count if the state of it right now is enabled.
                // This prevents issues with edge firelocks.
                if (_entManager.TryGetComponent<OccluderComponent>(child, out var occluder))
                {
                    valid |= occluder.Enabled;
                }
                // low walls & grilles
                valid |= _entManager.HasComponent<SharedCanBuildWindowOnTopComponent>(child);
                // cables
                valid |= _entManager.HasComponent<CableComponent>(child);
                // anything else that might need this forced
                valid |= tagSystem.HasTag(child, "ForceFixRotations");
                // override
                valid &= !tagSystem.HasTag(child, "ForceNoFixRotations");

                if (!valid)
                    continue;

                var childXform = xformQuery.GetComponent(child);

                if (childXform.LocalRotation != Angle.Zero)
                {
                    childXform.LocalRotation = Angle.Zero;
                    changed++;
                }
            }

            shell.WriteLine($"Changed {changed} entities. If things seem wrong, reconnect.");
        }
    }
}
