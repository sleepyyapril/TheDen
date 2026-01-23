using Content.Client.Eui;

namespace Content.Client._DEN.RecipeFinder.UI;

public sealed class BartenderRecipeFinderEui : BaseEui
{
    private BartenderRecipeFinderWindow Window { get; }

    public BartenderRecipeFinderEui()
    {
        Window = new();
    }

    public override void Opened()
    {
        base.Opened();
        Window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();
        Window.Close();
    }
}
