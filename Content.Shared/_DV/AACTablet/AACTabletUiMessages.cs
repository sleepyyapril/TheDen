using Content.Shared.DV_.QuickPhrase;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DV_.AACTablet;

[Serializable, NetSerializable]
public enum AACTabletKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class AACTabletSendPhraseMessage(List<ProtoId<QuickPhrasePrototype>> phraseIds) : BoundUserInterfaceMessage
{
    public List<ProtoId<QuickPhrasePrototype>> PhraseIds = phraseIds;
}
