// SPDX-FileCopyrightText: 2023 DrSmugleaf
// SPDX-FileCopyrightText: 2023 LordCarve
// SPDX-FileCopyrightText: 2023 metalgearsloth
// SPDX-FileCopyrightText: 2024 Krunklehorn
// SPDX-FileCopyrightText: 2024 Memeji
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers
// SPDX-FileCopyrightText: 2024 Plykiya
// SPDX-FileCopyrightText: 2024 VMSolidus
// SPDX-FileCopyrightText: 2024 nikthechampiongr
// SPDX-FileCopyrightText: 2025 Perry Fraser
// SPDX-FileCopyrightText: 2025 pathetic meowmeow
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Shared._DV.SmartFridge; // DeltaV - ough why do you not use events for this
using Content.Shared.Disposal;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Placeable;
using Content.Shared.Storage.Components;
using Content.Shared.Verbs;
using JetBrains.Annotations;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Storage.EntitySystems;

public sealed class DumpableSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDisposalUnitSystem _disposalUnitSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SmartFridgeSystem _fridge = default!; // DeltaV - ough why do you not use events for this

    private EntityQuery<ItemComponent> _itemQuery;

    public override void Initialize()
    {
        base.Initialize();
        _itemQuery = GetEntityQuery<ItemComponent>();
        SubscribeLocalEvent<DumpableComponent, AfterInteractEvent>(OnAfterInteract, after: new[]{ typeof(SharedEntityStorageSystem) });
        SubscribeLocalEvent<DumpableComponent, GetVerbsEvent<AlternativeVerb>>(AddDumpVerb);
        SubscribeLocalEvent<DumpableComponent, GetVerbsEvent<UtilityVerb>>(AddUtilityVerbs);
        SubscribeLocalEvent<DumpableComponent, DumpableDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(EntityUid uid, DumpableComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Handled)
            return;

        if (!_disposalUnitSystem.HasDisposals(args.Target) && !HasComp<PlaceableSurfaceComponent>(args.Target))
            return;

        if (!TryComp<StorageComponent>(uid, out var storage))
            return;

        if (!storage.Container.ContainedEntities.Any())
            return;

        StartDoAfter(uid, args.Target.Value, args.User, component);
        args.Handled = true;
    }

    private void AddDumpVerb(EntityUid uid, DumpableComponent dumpable, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<StorageComponent>(uid, out var storage) || !storage.Container.ContainedEntities.Any())
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                StartDoAfter(uid, args.Target, args.User, dumpable);//Had multiplier of 0.6f
            },
            Text = Loc.GetString("dump-verb-name"),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/drop.svg.192dpi.png")),
        };
        args.Verbs.Add(verb);
    }

    private void AddUtilityVerbs(EntityUid uid, DumpableComponent dumpable, GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<StorageComponent>(uid, out var storage) || !storage.Container.ContainedEntities.Any())
            return;

        if (_disposalUnitSystem.HasDisposals(args.Target) || HasComp<SmartFridgeComponent>(args.Target)) // DeltaV - ough why do you not use events for this
        {
            UtilityVerb verb = new()
            {
                Act = () =>
                {
                    StartDoAfter(uid, args.Target, args.User, dumpable);
                },
                Text = Loc.GetString("dump-disposal-verb-name", ("unit", args.Target)),
                IconEntity = GetNetEntity(uid)
            };
            args.Verbs.Add(verb);
        }

        if (HasComp<PlaceableSurfaceComponent>(args.Target))
        {
            UtilityVerb verb = new()
            {
                Act = () =>
                {
                    StartDoAfter(uid, args.Target, args.User, dumpable);
                },
                Text = Loc.GetString("dump-placeable-verb-name", ("surface", args.Target)),
                IconEntity = GetNetEntity(uid)
            };
            args.Verbs.Add(verb);
        }
    }

    private void StartDoAfter(EntityUid storageUid, EntityUid targetUid, EntityUid userUid, DumpableComponent dumpable)
    {
        if (!TryComp<StorageComponent>(storageUid, out var storage))
            return;

        var delay = 0f;

        foreach (var entity in storage.Container.ContainedEntities)
        {
            if (!_itemQuery.TryGetComponent(entity, out var itemComp) ||
                !_prototypeManager.TryIndex(itemComp.Size, out var itemSize))
            {
                continue;
            }

            delay += itemSize.Weight;
        }

        delay *= (float) dumpable.DelayPerItem.TotalSeconds * dumpable.Multiplier;

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, userUid, delay, new DumpableDoAfterEvent(), storageUid, target: targetUid, used: storageUid)
        {
            BreakOnMove = true,
            NeedHand = true
        });
    }

    private void OnDoAfter(EntityUid uid, DumpableComponent component, DumpableDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        DumpContents(uid, args.Args.Target, args.Args.User, component);
    }

    // DeltaV: Refactor to allow dumping that doesn't require a verb
    [PublicAPI]
    public void DumpContents(EntityUid uid, EntityUid? target, EntityUid user, DumpableComponent? component = null)
    {
        if (!TryComp<StorageComponent>(uid, out var storage) || !Resolve(uid, ref component))
            return;

        if (storage.Container.ContainedEntities.Count == 0)
            return;

        var dumpQueue = new Queue<EntityUid>(storage.Container.ContainedEntities);

        var dumped = false;

        if (_disposalUnitSystem.HasDisposals(target))
        {
            dumped = true;

            foreach (var entity in dumpQueue)
            {
                _disposalUnitSystem.DoInsertDisposalUnit(target.Value, entity, user);
            }
        }
        else if (HasComp<PlaceableSurfaceComponent>(target))
        {
            dumped = true;

            var (targetPos, targetRot) = _transformSystem.GetWorldPositionRotation(target.Value);

            foreach (var entity in dumpQueue)
            {
                _transformSystem.SetWorldPositionRotation(entity, targetPos + _random.NextVector2Box() / 4, targetRot);
            }
        }
        // Begin DeltaV - ough why do you not use events for this
        else if (TryComp<SmartFridgeComponent>(target, out var fridge))
        {
            dumped = true;
            _fridge.TryAddItem((target.Value, fridge), dumpQueue, user);
        }
        // End DeltaV - ough why do you not use events for this
        else
        {
            var targetPos = _transformSystem.GetWorldPosition(uid);

            foreach (var entity in dumpQueue)
            {
                var transform = Transform(entity);
                _transformSystem.SetWorldPositionRotation(entity, targetPos + _random.NextVector2Box() / 4, _random.NextAngle(), transform);
            }
        }

        if (dumped)
        {
            _audio.PlayPredicted(component.DumpSound, uid, user);
        }
    }
}
