using Content.Server.Consent;
using Content.Shared._DEN.Devourable;
using Content.Shared.Consent;
using Content.Shared.GameTicking;
using Content.Shared.Mobs;
using Robust.Shared.Prototypes;


namespace Content.Server._DEN.Devourable;


/// <summary>
/// This handles...
/// </summary>
public sealed class DevourableSystem : EntitySystem
{
    [Dependency] private readonly ConsentSystem _consentSystem = default!;

    private readonly ProtoId<ConsentTogglePrototype> _noDragonDevour = "NoDragonDevour";

    private EntityQueryEnumerator<DevourableComponent> _queryEnumerator;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawned);
        SubscribeLocalEvent<DevourableComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(Entity<DevourableComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
        {
            ent.Comp.AttemptedDevouring = false;
            Dirty(ent);
        }
    }

    private void OnPlayerSpawned(PlayerSpawnCompleteEvent ev)
    {
        var devourable = EnsureComp<DevourableComponent>(ev.Mob);
        UpdateConsent((ev.Mob, devourable));
    }

    public override void Update(float frameTime)
    {
        while (_queryEnumerator.MoveNext(out var uid, out var component))
            UpdateConsent((uid, component));
    }

    private void UpdateConsent(Entity<DevourableComponent> ent)
    {
        // we're checking if they have it on (no devour)
        var dragonDevour = !_consentSystem.HasConsent(ent.Owner, _noDragonDevour);

        if (ent.Comp.IsDevourable != dragonDevour)
        {
            ent.Comp.IsDevourable = dragonDevour;
            Dirty(ent);
        }
    }
}
