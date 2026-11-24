using BBStats.Models.UI;
using Microsoft.AspNetCore.Mvc;

namespace BBStats.Pages.Player;

public static class PlayerPageUrl
{
    public static string? For(
        IUrlHelper url,
        string steamId,
        string character,
        int page = 1,
        MatchSeriesFilter? filter = null)
    {
        filter = (filter ?? MatchSeriesFilter.None).Normalize();

        return url.Page("/Player/Index", new
        {
            steamId,
            character,
            matchPage = page <= 1 ? (int?)null : page,
            minElo = filter.MinOpponentElo,
            maxElo = filter.MaxOpponentElo,
            opponent = filter.OpponentName,
            opponentCharacter = filter.OpponentCharacterSlug
        });
    }
}
