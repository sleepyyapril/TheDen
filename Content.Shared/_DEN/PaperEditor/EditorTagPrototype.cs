using Robust.Shared.Prototypes;


namespace Content.Shared._DEN.PaperEditor;


/// <summary>
/// This is a prototype for tags in the paper editor.
/// </summary>
[Prototype()]
public sealed partial class EditorTagPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// The tag to surround the text with or surround the cursor with.
    /// Example: head=1 would become [head=1] and end with [/head].
    /// </summary>
    [DataField]
    public string TagText { get; set; } = string.Empty;

    /// <summary>
    /// The text the button would have.
    /// </summary>
    [DataField]
    public string ButtonText { get; set; } = string.Empty;

    /// <summary>
    /// The text that would pop up when hovering over the button.
    /// </summary>
    [DataField]
    public string Tooltip { get; set; } = string.Empty;
}
