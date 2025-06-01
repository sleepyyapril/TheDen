using Content.Shared.DV_.Biscuit;

namespace Content.Server.DV_.Biscuit;

[RegisterComponent]
public sealed partial class BiscuitComponent : SharedBiscuitComponent
{
    [DataField]
    public bool Cracked { get; set; }
}
