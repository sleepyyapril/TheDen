using System.Linq;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Shared.Administration;


namespace Content.Server._DEN.Discord;


public sealed partial class DiscordUserLink
{
    private async void OnPlayerJoined(PlayerJoinedLobbyEvent ev)
    {
        var actualRank = _adminManager.GetAdminRankId(ev.PlayerSession);
        var expectedRank = GetAdminRankOfRole(ev.PlayerSession.UserId);

        if (expectedRank == actualRank)
            return;

        var admin = await _db.GetAdminDataForAsync(ev.PlayerSession.UserId);

        if (expectedRank == null && admin != null)
        {
            await _db.RemoveAdminAsync(ev.PlayerSession.UserId);
            return;
        }

        var (bad, _) = await FetchAndCheckRank(expectedRank);

        if (bad)
        {
            return;
        }

        var realAdmin = admin ?? new Admin();
        realAdmin.AdminRankId = expectedRank;

        await _db.UpdateAdminAsync(realAdmin);
    }

    private async Task<(bool bad, string?)> FetchAndCheckRank(int? rankId)
    {
        string? ret = null;

        if (rankId is not { } r)
            return (true, null);

        var rank = await _db.GetAdminRankAsync(r);
        if (rank == null)
            return (true, null);

        ret = rank.Name;

        return (false, ret);
    }
}
