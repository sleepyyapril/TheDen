// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 sleepyyapril <flyingkarii@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Client.Chemistry.EntitySystems;
using Content.Server.Chemistry.ReactionEffects;
using Content.Server.EntityEffects.Effects;
using Content.Server.Nutrition.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Kitchen;
using Content.Shared.Nutrition.Components;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Nutrition.EntitySystems;

public sealed class FoodGuideDataSystem : SharedFoodGuideDataSystem
{
    public static readonly ProtoId<ReagentPrototype>[] ReagentWhitelist =
    [
        "Nutriment",
        "Vitamin",
        "Protein",
        "UncookedAnimalProteins",
        "Fat",
        "Water"
    ];

    public static readonly string[] ComponentNamesBlacklist = ["HumanoidAppearance",];

    public static readonly string[] SuffixBlacklist = ["debug", "do not map", "admeme",];

    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    private readonly Dictionary<string, List<FoodSourceData>> _sources = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
        _player.PlayerStatusChanged += OnPlayerStatusChanged;

        ReloadRecipes();
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<EntityPrototype>()
            && !args.WasModified<FoodRecipePrototype>()
            && !args.WasModified<ReactionPrototype>()
        )
            return;

        ReloadRecipes();
    }

    public void ReloadRecipes()
    {
        // TODO: add this code to the list of known recipes because this is spaghetti
        _sources.Clear();

        // Butcherable and slicable entities
        foreach (var ent in _protoMan.EnumeratePrototypes<EntityPrototype>())
        {
            if (ent.Abstract
                || ent.Components.Any(it => ComponentNamesBlacklist.Contains(it.Key))
                || ent.SetSuffix is {} suffix && SuffixBlacklist.Any(it => suffix.Contains(it, StringComparison.OrdinalIgnoreCase))
            )
                continue;

            if (ent.TryGetComponent<ButcherableComponent>(out var butcherable, _componentFactory))
            {
                var butcheringSource = new FoodButcheringData(ent, butcherable);
                foreach (var butchlet in butcherable.SpawnedEntities)
                {
                    if (butchlet.PrototypeId is null)
                        continue;

                    _sources.GetOrNew(butchlet.PrototypeId).Add(butcheringSource);
                }
            }

            if (ent.TryGetComponent<SliceableFoodComponent>(out var sliceable, _componentFactory) && sliceable.Slice is not null)
            {
                _sources.GetOrNew(sliceable.Slice).Add(new FoodSlicingData(ent, sliceable.Slice.Value, sliceable.TotalCount));
            }
        }

        // Recipes
        foreach (var recipe in _protoMan.EnumeratePrototypes<FoodRecipePrototype>())
        {
            _sources.GetOrNew(recipe.Result).Add(new FoodRecipeData(recipe));
        }

        // Entity-spawning reactions
        foreach (var reaction in _protoMan.EnumeratePrototypes<ReactionPrototype>())
        {
            foreach (var effect in reaction.Effects)
            {
                if (effect is not CreateEntityReactionEffect entEffect)
                    continue;

                _sources.GetOrNew(entEffect.Entity).Add(new FoodReactionData(reaction, entEffect.Entity, (int) entEffect.Number));
            }
        }

        Registry.Clear();

        foreach (var (result, sources) in _sources)
        {
            var proto = _protoMan.Index<EntityPrototype>(result);
            var composition = proto.TryGetComponent<FoodComponent>(out var food, _componentFactory) && proto.TryGetComponent<SolutionContainerManagerComponent>(out var manager)
                ? manager?.Solutions?[food.Solution]?.Contents?.ToArray() ?? []
                : [];

            // We filter out food without whitelisted reagents because well when people look for food they usually expect FOOD and not insulated gloves.
            // And we get insulated and other gloves because they have ButcherableComponent and they are also moth food
            if (!composition.Any(it => ReagentWhitelist.Contains<ProtoId<ReagentPrototype>>(it.Reagent.Prototype)))
                continue;

            // We also limit the number of sources to 10 because it's a huge performance strain to render 500 raw meat recipes.
            var distinctSources = sources.DistinctBy(it => it.Identitier).Take(10);
            var entry = new FoodGuideEntry(result, proto.Name, distinctSources.ToArray(), composition);
            Registry.Add(entry);
        }

        RaiseNetworkEvent(new FoodGuideRegistryChangedEvent(Registry));
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.NewStatus != SessionStatus.Connected)
            return;

        RaiseNetworkEvent(new FoodGuideRegistryChangedEvent(Registry), args.Session);
    }
}
