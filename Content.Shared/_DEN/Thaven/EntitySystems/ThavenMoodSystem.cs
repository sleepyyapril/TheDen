using Content.Shared._Impstation.Thaven.Components;
using Content.Shared.Bed.Sleep;
using Content.Shared.Emag.Systems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Shared._Impstation.Thaven;

public abstract partial class SharedThavenMoodSystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private void OnAttemptEmag(Entity<ThavenMoodsComponent> ent, ref OnAttemptEmagEvent args)
    {
        var user = args.UserUid;

        // Thaven must be Not Awake or you must be emagging yourself
        if (user != ent.Owner
            && !HasComp<SleepingComponent>(ent)
            && _mobState.IsIncapacitated(ent))
        {
            _popup.PopupClient(Loc.GetString("emag-thaven-alive"), user, user);
            args.Handled = true;
        }
    }
}
