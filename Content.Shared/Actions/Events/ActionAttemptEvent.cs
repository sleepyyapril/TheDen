// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Actions.Events;

/// <summary>
/// Raised before an action is used and can be cancelled to prevent it.
/// Allowed to have side effects like modifying the action component.
/// </summary>
[ByRefEvent]
public record struct ActionAttemptEvent(EntityUid User, bool Cancelled = false);
