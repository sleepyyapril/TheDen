// SPDX-FileCopyrightText: 2025 Mnemotechnican
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.RegularExpressions;
using Content.Shared._Floof.Consent;
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Managers;
using Content.Shared.Examine;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;


namespace Content.Shared._Floof.Examine;


public abstract class SharedCustomExamineSystem : EntitySystem
{
    public static ProtoId<ConsentTogglePrototype> NsfwDescConsent = "NSFWDescriptions";
    public static int MaxLength = 256;
    /// <summary>Max length of any content field, INCLUDING markup.</summary>
    public static int AbsolutelyMaxLength = 1024;

    private static readonly Regex BadMarkupRegex = new("\\[.*?head.*?\\]", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(5));

    [Dependency] private readonly SharedConsentSystem _consent = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly ISharedAdminManager _admin = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CustomExamineComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<CustomExamineComponent> ent, ref ExaminedEvent args)
    {
        CheckExpirations(ent);

        if (ent.Comp.Data.Count == 0)
            return;

        using (args.PushGroup(nameof(CustomExamineComponent), -1))
        {
            foreach (var dataItem in ent.Comp.Data)
            {
                var allowNsfw = _consent.HasConsent(args.Examiner, NsfwDescConsent);
                var hasContent = dataItem.Content is not null;
                var contentHidden = hasContent && dataItem.RequiresConsent && !allowNsfw;

                // If subtle is shown, then public is guaranteed to also be shown - this is to avoid extra raycasts
                var rangeHidden = hasContent && !_examine.InRangeUnOccluded(
                    args.Examiner,
                    args.Examined,
                    dataItem.VisibilityRange);

                if (hasContent && !contentHidden && !rangeHidden)
                    args.PushMarkup(dataItem.Content!);

                // If something is hidden due to consent preferences, add a note (but only if in range)
                if (hasContent && !rangeHidden && contentHidden)
                    args.PushMarkup(Loc.GetString("custom-examine-nsfw-hidden"));
            }
        }
    }

    public bool CanChangeExamine(EntityUid examining, EntityUid examinee)
    {
        return examining == examinee && _actionBlocker.CanConsciouslyPerformAction(examinee);
    }

    private void CheckExpirations(Entity<CustomExamineComponent> ent)
    {
        bool Check(CustomExamineData data)
        {
            if (data.Content is null
                || data.ExpireTime.Ticks <= 0
                || data.ExpireTime > _timing.CurTime)
                return false;

            data.Content = null;
            return true;
        }

        var shouldDirty = false;

        foreach (var dataItem in ent.Comp.Data)
        {
            if (!Check(dataItem))
                continue;

            shouldDirty = true;
        }

        if (shouldDirty)
            Dirty(ent);
    }

    protected CustomExamineData? TrimData(CustomExamineData data)
    {
        if (data.Content is null)
            return null;

        // Exclude forbidden markup. Unlike ss14's chat cleanup code, this should also remove nested markup.
        data.Content = BadMarkupRegex.Replace(data.Content, "<bad markup>").Trim();

        // Shitty way to preserve and ignore markup while trimming
        var markupLength = MarkupLength(data.Content);
        if (data.Content.Length > AbsolutelyMaxLength)
            data.Content = data.Content[..AbsolutelyMaxLength];
        if (data.Content.Length - markupLength > MaxLength)
            data.Content = data.Content[..(MaxLength - markupLength)];

        if (data.Content.Length == 0)
            data.Content = null;

        return data;
    }

    protected int LengthWithoutMarkup(string text) => FormattedMessage.RemoveMarkupPermissive(text).Length;

    protected int MarkupLength(string text) => text.Length - LengthWithoutMarkup(text);
}
