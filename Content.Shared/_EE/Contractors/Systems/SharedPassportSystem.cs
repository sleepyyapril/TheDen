// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <flyingkarii@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._EE.Contractors.Components;
using Content.Shared._EE.Contractors.Prototypes;
using Content.Shared.Administration.Logs;
using Content.Shared.Clothing.Loadouts.Systems;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Preferences;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;


namespace Content.Shared._EE.Contractors.Systems;

public class SharedPassportSystem : EntitySystem
{
    public const int CurrentYear = 2450;
    const string PIDChars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789";

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedTransformSystem _sharedTransformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PassportComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<PlayerLoadoutAppliedEvent>(OnPlayerLoadoutApplied);
        SubscribeLocalEvent<PassportComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, PassportComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange
            || component.IsClosed
            || component.OwnerProfile == null)
            return;

        var species = _prototypeManager.Index<SpeciesPrototype>(component.OwnerProfile.Species);

        args.PushMarkup($"Registered to: {component.OwnerProfile.Name}", 50);
        args.PushMarkup($"Species: {Loc.GetString(species.Name)}", 49);
        args.PushMarkup($"Sex: {component.OwnerProfile.Gender}", 48);
        args.PushMarkup($"Height: {MathF.Round(component.OwnerProfile.Height * species.AverageHeight)} cm", 47);
        args.PushMarkup($"Year of Birth: {CurrentYear - component.OwnerProfile.Age}", 46);

        args.PushMarkup(
            $"PID: {GenerateIdentityString(component.OwnerProfile.Name
            + component.OwnerProfile.Height
            + component.OwnerProfile.Age
            + component.OwnerProfile.Height
            + component.OwnerProfile.FlavorText)}",
            45);
    }

    private void OnPlayerLoadoutApplied(PlayerLoadoutAppliedEvent ev)
    {
        if (Deleted(ev.Mob) || !Exists(ev.Mob))
            return;

        if (!_prototypeManager.TryIndex(
            ev.Profile.Nationality,
            out NationalityPrototype? nationalityPrototype) || !_prototypeManager.TryIndex(nationalityPrototype.PassportPrototype, out EntityPrototype? entityPrototype))
            return;

        var passportEntity = _entityManager.SpawnEntity(entityPrototype.ID, _sharedTransformSystem.GetMapCoordinates(ev.Mob));
        var passportComponent = _entityManager.GetComponent<PassportComponent>(passportEntity);

        UpdatePassportProfile(new(passportEntity, passportComponent), ev.Profile);

        // Try to find back-mounted storage apparatus
        if (_inventory.TryGetSlotEntity(ev.Mob, "back", out var item) &&
                EntityManager.TryGetComponent<StorageComponent>(item, out var inventory))
            // Try inserting the entity into the storage, if it can't, it leaves the loadout item on the ground
        {
            if (!EntityManager.TryGetComponent<ItemComponent>(passportEntity, out var itemComp)
                || !_storage.CanInsert(item.Value, passportEntity, out _, inventory, itemComp)
                || !_storage.Insert(item.Value, passportEntity, out _, playSound: false))
            {
                /** _entityManager.System<SharedAdminLogSystem>().Add(
                    LogType.EntitySpawn,
                    LogImpact.Low,
                    $"Passport for {ev.Profile.Name} was spawned on the floor due to missing bag space");
                    **/
            }
        }
    }

    public void UpdatePassportProfile(Entity<PassportComponent> passport, HumanoidCharacterProfile profile)
    {
        passport.Comp.OwnerProfile = profile;
        var evt = new PassportProfileUpdatedEvent(profile);
        RaiseLocalEvent(passport, ref evt);
    }

    private void OnUseInHand(Entity<PassportComponent> passport, ref UseInHandEvent evt)
    {
        if (evt.Handled || !_timing.IsFirstTimePredicted)
            return;

        evt.Handled = true;
        passport.Comp.IsClosed = !passport.Comp.IsClosed;

        var passportEvent = new PassportToggleEvent();
        RaiseLocalEvent(passport, ref passportEvent);
    }

    private static string GenerateIdentityString(string seed)
    {
        var hashCode = seed.GetHashCode();
        System.Random random = new System.Random(hashCode);

        char[] result = new char[17]; // 15 characters + 2 dashes

        int j = 0;
        for (int i = 0; i < 15; i++)
        {
            if (i == 5 || i == 10)
            {
                result[j++] = '-';
            }
            result[j++] = PIDChars[random.Next(PIDChars.Length)];
        }

        return new string(result);
    }

    [ByRefEvent]
    public sealed class PassportToggleEvent : HandledEntityEventArgs {}

    [ByRefEvent]
    public sealed class PassportProfileUpdatedEvent(HumanoidCharacterProfile profile) : HandledEntityEventArgs
    {
        public HumanoidCharacterProfile Profile { get; } = profile;
    }
}
