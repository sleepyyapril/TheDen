// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Client._Shitmed.UserInterface.Systems.PartStatus;
using Content.Shared._Shitmed.Targeting;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Client._Shitmed.UserInterface.Systems.PartStatus.Widgets;

[GenerateTypedNameReferences]
public sealed partial class PartStatusControl : UIWidget
{
    private readonly Dictionary<TargetBodyPart, TextureRect> _partStatusControls;
    private readonly PartStatusUIController _controller;
    public PartStatusControl()
    {
        RobustXamlLoader.Load(this);

        _controller = UserInterfaceManager.GetUIController<PartStatusUIController>();
        _partStatusControls = new Dictionary<TargetBodyPart, TextureRect>
        {
            { TargetBodyPart.Head, DollHead },
            { TargetBodyPart.Torso, DollTorso },
            { TargetBodyPart.Groin, DollGroin },
            { TargetBodyPart.LeftArm, DollLeftArm },
            { TargetBodyPart.LeftHand, DollLeftHand },
            { TargetBodyPart.RightArm, DollRightArm },
            { TargetBodyPart.RightHand, DollRightHand },
            { TargetBodyPart.LeftLeg, DollLeftLeg },
            { TargetBodyPart.LeftFoot, DollLeftFoot },
            { TargetBodyPart.RightLeg, DollRightLeg },
            { TargetBodyPart.RightFoot, DollRightFoot }
        };
    }

    public void SetTextures(Dictionary<TargetBodyPart, TargetIntegrity> state)
    {
        foreach (var (bodyPart, integrity) in state)
        {
            string enumName = Enum.GetName(typeof(TargetBodyPart), bodyPart) ?? "Unknown";
            int enumValue = (int) integrity;
            var texture = new SpriteSpecifier.Rsi(new ResPath($"/Textures/_Shitmed/Interface/Targeting/Status/{enumName.ToLowerInvariant()}.rsi"), $"{enumName.ToLowerInvariant()}_{enumValue}");
            _partStatusControls[bodyPart].Texture = _controller.GetTexture(texture);
        }
    }

    public void SetVisible(bool visible) => this.Visible = visible;

}
