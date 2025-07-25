// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 sleepyyapril <flyingkarii@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Language;
using Content.Shared.Language.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.Traits.Assorted;

/// <summary>
///     When applied to a not-yet-spawned player entity, removes <see cref="BaseLanguage"/> from the lists of their languages
///     and gives them a translator instead.
/// </summary>
[RegisterComponent]
public sealed partial class ForeignerTraitComponent : Component
{
    /// <summary>
    ///     The "base" language that is to be removed and substituted with a translator.
    ///     By default, equals to the fallback language, which is TauCetiBasic.
    /// </summary>
    [DataField]
    public ProtoId<LanguagePrototype> BaseLanguage = SharedLanguageSystem.FallbackLanguagePrototype;

    /// <summary>
    ///     Whether this trait prevents the entity from understanding the base language.
    /// </summary>
    [DataField]
    public bool CantUnderstand = true;

    /// <summary>
    ///     Whether this trait prevents the entity from speaking the base language.
    /// </summary>
    [DataField]
    public bool CantSpeak = true;

    /// <summary>
    ///     The base translator prototype to use when creating a translator for the entity.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId BaseTranslator = default!;
}
