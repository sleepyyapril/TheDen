namespace Content.Server._DEN.Speech.Components;


/// <summary>
/// Every character is replaced by the opposite character in this accent.
/// </summary>
[RegisterComponent]
[Access(typeof(OppositeAccentComponent))]
public sealed partial class OppositeAccentComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public string AccentId = "opposite";
}
