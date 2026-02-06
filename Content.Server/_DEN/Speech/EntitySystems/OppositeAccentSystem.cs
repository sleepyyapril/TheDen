using Content.Server._DEN.Speech.Components;
using Content.Server.Speech;
using Content.Server.Speech.EntitySystems;


namespace Content.Server._DEN.Speech.EntitySystems;


/// <summary>
/// Every character is replaced by the opposite character in this accent.
/// </summary>
public sealed class OppositeAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OppositeAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public string Accentuate(Entity<OppositeAccentComponent> ent, string message) =>
        _replacement.ApplyReplacements(message, ent.Comp.AccentId);

    private void OnAccentGet(Entity<OppositeAccentComponent> ent, ref AccentGetEvent args) =>
        args.Message = Accentuate(ent, args.Message);
}
