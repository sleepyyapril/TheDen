// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Nutrition.FoodMetamorphRules;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared.Nutrition.Prototypes;

/// <summary>
/// Stores a recipe so that FoodSequence assembled in the right sequence can turn into a special meal.
/// </summary>
[Prototype]
public sealed partial class MetamorphRecipePrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// The key of the FoodSequence being collected. For example “burger” “taco” etc.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<TagPrototype> Key = string.Empty;

    /// <summary>
    /// The entity that will be created as a result of this recipe, and into which all the reagents will be transferred.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Result = default!;

    /// <summary>
    /// A sequence of rules that must be followed for FoodSequence to metamorphose into a special food.
    /// </summary>
    [DataField]
    public List<FoodMetamorphRule> Rules = new();
}
