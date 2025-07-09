using Content.Shared.Damage;
using Robust.Shared.Timing;


namespace Content.Server._DEN.Damage;


/// <summary>
/// This handles...
/// </summary>
public sealed class GradualDamageSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private List<GradualDamage> _gradualDamage = new();
    private TimeSpan _lastUpdate = TimeSpan.Zero;
    private TimeSpan _interval = TimeSpan.FromSeconds(1);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Only update once a second.
        if (_lastUpdate + _interval > _gameTiming.CurTime)
            return;

        for (var i = 0; i < _gradualDamage.Count; i++)
        {
            var gradualDamage = _gradualDamage[i];
            gradualDamage.Seconds--;

            if (gradualDamage.Seconds <= 0)
                _gradualDamage.RemoveAt(i);

            DoGradualDamage(gradualDamage);
        }
    }

    private void DoGradualDamage(GradualDamage gradualDamage)
    {
        var damage = gradualDamage.Damage ?? new DamageSpecifier();
        _damageable.TryChangeDamage(gradualDamage.Uid, damage, gradualDamage.IgnoreResistances);
    }

    public void TryAddDamage(EntityUid uid, int seconds, DamageSpecifier? damagePerSecond, bool ignoreResistances = false)
    {
        if (!HasComp<DamageableComponent>(uid))
            return;

        var gradual = new GradualDamage()
        {
            Uid = uid,
            Seconds = seconds,
            Damage = damagePerSecond,
            IgnoreResistances = ignoreResistances,
        };
        _gradualDamage.Add(gradual);
    }

    public void AddDamage(EntityUid uid, int seconds, DamageSpecifier? damagePerSecond, bool ignoreResistances = false)
    {
        var gradual = new GradualDamage()
        {
            Uid = uid,
            Seconds = seconds,
            Damage = damagePerSecond,
            IgnoreResistances = ignoreResistances,
        };

        _gradualDamage.Add(gradual);
    }

    record struct GradualDamage
    {
        public EntityUid Uid { get; set; }
        public int Seconds { get; set; }
        public DamageSpecifier? Damage { get; set; }
        public bool IgnoreResistances { get; set; }
    }
}
