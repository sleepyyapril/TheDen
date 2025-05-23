using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.EntityEffects.Effects;

/// <summary>
///     Tries to force someone to emote (scream, laugh, etc). Still respects whitelists/blacklists and other limits of the specified emote unless forced.
/// </summary>
public sealed partial class Emote : EventEntityEffect<Emote>
{
    [DataField("emote", customTypeSerializer: typeof(PrototypeIdSerializer<EmotePrototype>))]
    public string? EmoteId;

    [DataField]
    public bool ShowInChat;

    [DataField]
    public bool Force = false;

    // JUSTIFICATION: Emoting is flavor, so same reason popup messages are not in here.
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if (!ShowInGuidebook)
            return null; // JUSTIFICATION: Emoting is mostly flavor, so same reason popup messages are not in here.

        return Loc.GetString("reagent-effect-guidebook-emote", ("chance", Probability), ("emote", EmoteId));
    }
}
