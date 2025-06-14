using Content.Shared.Alert;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Rejuvenate;
using Content.Shared.StatusIcon;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Content.Shared.Mood;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;

namespace Content.Shared.Nutrition.EntitySystems;

[UsedImplicitly]
public sealed class ThirstSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedJetpackSystem _jetpack = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    [ValidatePrototypeId<SatiationIconPrototype>]
    private const string ThirstIconOverhydratedId = "ThirstIconOverhydrated";

    [ValidatePrototypeId<SatiationIconPrototype>]
    private const string ThirstIconThirstyId = "ThirstIconThirsty";

    [ValidatePrototypeId<SatiationIconPrototype>]
    private const string ThirstIconParchedId = "ThirstIconParched";

    private SatiationIconPrototype? _thirstIconOverhydrated = null;
    private SatiationIconPrototype? _thirstIconThirsty = null;
    private SatiationIconPrototype? _thirstIconParched = null;

    public override void Initialize()
    {
        base.Initialize();

        DebugTools.Assert(_prototype.TryIndex(ThirstIconOverhydratedId, out _thirstIconOverhydrated) &&
                          _prototype.TryIndex(ThirstIconThirstyId, out _thirstIconThirsty) &&
                          _prototype.TryIndex(ThirstIconParchedId, out _thirstIconParched));

        SubscribeLocalEvent<ThirstComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
        SubscribeLocalEvent<ThirstComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ThirstComponent, RejuvenateEvent>(OnRejuvenate);
    }

    private void OnMapInit(EntityUid uid, ThirstComponent component, MapInitEvent args)
    {
        // Do not change behavior unless starting value is explicitly defined
        if (component.CurrentThirst < 0)
        {
            component.CurrentThirst = _random.Next(
                (int) component.ThirstThresholds[ThirstThreshold.Thirsty] + 10,
                (int) component.ThirstThresholds[ThirstThreshold.Okay] - 1);
        }
        component.NextUpdateTime = _timing.CurTime;
        component.CurrentThirstThreshold = GetThirstThreshold(component, component.CurrentThirst);
        component.LastThirstThreshold = ThirstThreshold.Okay; // TODO: Potentially change this -> Used Okay because no effects.
        // TODO: Check all thresholds make sense and throw if they don't.
        UpdateEffects(uid, component);

        TryComp(uid, out MovementSpeedModifierComponent? moveMod);
            _movement.RefreshMovementSpeedModifiers(uid, moveMod);
    }

    private void OnRefreshMovespeed(EntityUid uid, ThirstComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        // TODO: This should really be taken care of somewhere else
        if (_config.GetCVar(CCVars.MoodEnabled)
            || _jetpack.IsUserFlying(uid))
            return;

        var mod = component.CurrentThirstThreshold <= ThirstThreshold.Parched ? 0.75f : 1.0f;
        args.ModifySpeed(mod, mod);
    }

    private void OnRejuvenate(EntityUid uid, ThirstComponent component, RejuvenateEvent args)
    {
        SetThirst(uid, component, component.ThirstThresholds[ThirstThreshold.Okay]);
    }

    private ThirstThreshold GetThirstThreshold(ThirstComponent component, float amount)
    {
        ThirstThreshold result = ThirstThreshold.Dead;
        var value = component.ThirstThresholds[ThirstThreshold.OverHydrated];
        foreach (var threshold in component.ThirstThresholds)
        {
            if (threshold.Value <= value && threshold.Value >= amount)
            {
                result = threshold.Key;
                value = threshold.Value;
            }
        }

        return result;
    }

    public void ModifyThirst(EntityUid uid, ThirstComponent component, float amount)
    {
        SetThirst(uid, component, component.CurrentThirst + amount);
    }

    public void SetThirst(EntityUid uid, ThirstComponent component, float amount)
    {
        component.CurrentThirst = Math.Clamp(amount,
            component.ThirstThresholds[ThirstThreshold.Dead],
            component.ThirstThresholds[ThirstThreshold.OverHydrated]
        );
        Dirty(uid, component);
    }

    private bool IsMovementThreshold(ThirstThreshold threshold)
    {
        switch (threshold)
        {
            case ThirstThreshold.Dead:
            case ThirstThreshold.Parched:
                return true;
            case ThirstThreshold.Thirsty:
            case ThirstThreshold.Okay:
            case ThirstThreshold.OverHydrated:
                return false;
            default:
                throw new ArgumentOutOfRangeException(nameof(threshold), threshold, null);
        }
    }

    public bool TryGetStatusIconPrototype(ThirstComponent component, out SatiationIconPrototype? prototype)
    {
        switch (component.CurrentThirstThreshold)
        {
            case ThirstThreshold.OverHydrated:
                prototype = _thirstIconOverhydrated;
                return true;

            case ThirstThreshold.Thirsty:
                prototype = _thirstIconThirsty;
                return true;

            case ThirstThreshold.Parched:
                prototype = _thirstIconParched;
                return true;

            default:
                prototype = null;
                return false;
        }
    }

    private void UpdateEffects(EntityUid uid, ThirstComponent component)
    {
        if (!_config.GetCVar(CCVars.MoodEnabled)
            && IsMovementThreshold(component.LastThirstThreshold) != IsMovementThreshold(component.CurrentThirstThreshold)
            && TryComp(uid, out MovementSpeedModifierComponent? movementSlowdownComponent))
        {
            _movement.RefreshMovementSpeedModifiers(uid, movementSlowdownComponent);
        }

        // Update UI
        if (ThirstComponent.ThirstThresholdAlertTypes.TryGetValue(component.CurrentThirstThreshold, out var alertId))
        {
            _alerts.ShowAlert(uid, alertId);
        }
        else
        {
            _alerts.ClearAlertCategory(uid, component.ThirstyCategory);
        }

        var ev = new MoodEffectEvent("Thirst" + component.CurrentThirstThreshold);
        RaiseLocalEvent(uid, ev);

        if (component.CurrentThirstThreshold == ThirstThreshold.Dead)
            return;

        component.ActualDecayRate = component.BaseDecayRate * component.DecayRateMultiplier;
        if (!component.ThirstThresholdDecayModifiers.TryGetValue(component.CurrentThirstThreshold,
            out var decayRateModifier))
        {
            Log.Error($"No thirst threshold found for {component.CurrentThirstThreshold}");
            throw new ArgumentOutOfRangeException($"No thirst threshold found for {component.CurrentThirstThreshold}");
        }

        component.ActualDecayRate *= decayRateModifier;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ThirstComponent>();
        while (query.MoveNext(out var uid, out var thirst))
        {
            if (_timing.CurTime < thirst.NextUpdateTime)
                continue;

            thirst.NextUpdateTime += thirst.UpdateRate;

            ModifyThirst(uid, thirst, -thirst.ActualDecayRate);
            var calculatedThirstThreshold = GetThirstThreshold(thirst, thirst.CurrentThirst);

            if (calculatedThirstThreshold == thirst.CurrentThirstThreshold)
                continue;

            thirst.CurrentThirstThreshold = calculatedThirstThreshold;
            UpdateEffects(uid, thirst);
        }
    }
}
