// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 88tv <131759102+88tv@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 lzk <124214523+lzk228@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Clothing;

namespace Content.Shared.Chat.TypingIndicator;

/// <summary>
///     Sync typing indicator icon between client and server.
/// </summary>
public abstract class SharedTypingIndicatorSystem : EntitySystem
{
    /// <summary>
    ///     Default ID of <see cref="TypingIndicatorPrototype"/>
    /// </summary>
    [ValidatePrototypeId<TypingIndicatorPrototype>]
    public const string InitialIndicatorId = "default";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TypingIndicatorClothingComponent, ClothingGotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<TypingIndicatorClothingComponent, ClothingGotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        // when player poses entity we want to make sure that there is typing indicator
        EnsureComp<TypingIndicatorComponent>(ev.Entity);
        // we also need appearance component to sync visual state
        EnsureComp<AppearanceComponent>(ev.Entity);
    }

    private void OnPlayerDetached(EntityUid uid, TypingIndicatorComponent component, PlayerDetachedEvent args)
    {
        // player left entity body - hide typing indicator
        SetTypingIndicatorState(uid, TypingIndicatorState.None);
    }

    private void OnGotEquipped(Entity<TypingIndicatorClothingComponent> entity, ref ClothingGotEquippedEvent args)
    {
        if (!TryComp<TypingIndicatorComponent>(args.Wearer, out var indicator))
            return;
        }

        // check if this entity can speak or emote
        if (!_actionBlocker.CanEmote(uid.Value) && !_actionBlocker.CanSpeak(uid.Value))
        {
            // nah, make sure that typing indicator is disabled
            SetTypingIndicatorState(uid.Value, TypingIndicatorState.None);
            return;
        }

        SetTypingIndicatorState(uid.Value, ev.State);
    }

    private void SetTypingIndicatorState(EntityUid uid, TypingIndicatorState state, AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref appearance, false))
            return;

        _appearance.SetData(uid, TypingIndicatorVisuals.State, state, appearance);
    }
}
