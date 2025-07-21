// SPDX-FileCopyrightText: 2025 Mnemotechnican
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Net;
using Content.Server.Database;
using Content.Shared.CCVar;
using Content.Shared.FloofStation.Info;
using Robust.Shared.Configuration;
using Robust.Shared.Network;


namespace Content.Server._Floof.Info;


public sealed class NsfwDisclaimerSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IServerDbManager _db = default!;

    private static DateTime LastValidReadTime => DateTime.UtcNow - TimeSpan.FromDays(60);

    public override void Initialize()
    {
        SubscribeNetworkEvent<PopupDisclaimerResponseMessage>(OnPopupDisclaimerResponse);

        _netManager.Connected += OnConnected;
    }

    private void OnPopupDisclaimerResponse(PopupDisclaimerResponseMessage msg, EntitySessionEventArgs args)
    {
        if (!msg.Response)
            args.SenderSession.Channel.Disconnect("User rejected the disclaimer");

        _db.SetAcceptedPrompt(args.SenderSession.UserId, msg.Response);
    }

    private async void OnConnected(object? sender, NetChannelArgs e)
    {
        // Ignore localhost unless the specified debug cvar is set
        if (IPAddress.IsLoopback(e.Channel.RemoteEndPoint.Address) && _cfg.GetCVar(CCVars.RulesExemptLocal))
            return;

        var message = new ShowNsfwPopupDisclaimerMessage();
        RaiseNetworkEvent(message, e.Channel);
    }
}
