// SPDX-FileCopyrightText: 2025 Eris <eris@erisws.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Client.UserInterface.Controls;
using Content.Shared.Clothing.Components;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;


namespace Content.Client._Goobstation.Clothing;

public sealed class ToggleableClothingBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    private SimpleRadialMenu? _menu;

    public ToggleableClothingBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        if (!EntMan.TryGetComponent<ToggleableClothingComponent>(Owner, out var clothing)
            || clothing.Container is not { } clothingContainer)
            return;

        var actions = GetToggleableClothingActions(clothing, clothingContainer);

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        _menu.SetButtons(actions);
        _menu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuActionOption<NetEntity>> GetToggleableClothingActions(
        ToggleableClothingComponent clothing,
        Container clothingContainer)
    {
        var actions = new List<RadialMenuActionOption<NetEntity>>();

        foreach (var pair in clothing.ClothingUids)
        {
            // Change tooltip text if attached clothing is toggle/untoggled
            var tooltipText = Loc.GetString(clothing.UnattachTooltip);

            if (clothingContainer.Contains(pair.Key))
                tooltipText = Loc.GetString(clothing.AttachTooltip);

            var netEntity = EntMan.GetNetEntity(Owner);
            var action = new RadialMenuActionOption<NetEntity>(SendToggleableClothingMessage, netEntity)
            {
                ToolTip = tooltipText,
                IconSpecifier = RadialMenuIconSpecifier.With(pair.Key)
            };

            actions.Add(action);
        }

        return actions;
    }

    private void SendToggleableClothingMessage(NetEntity uid)
    {
        var message = new ToggleableClothingUiMessage(uid);
        SendPredictedMessage(message);
    }
}
