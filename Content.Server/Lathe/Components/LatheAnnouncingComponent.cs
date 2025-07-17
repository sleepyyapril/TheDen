using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server.Lathe.Components;


/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class LatheAnnouncingComponent : Component
{
    /// <summary>
    /// Radio channels to broadcast to when a new set of recipes is received
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<RadioChannelPrototype>> Channels = new();
}
