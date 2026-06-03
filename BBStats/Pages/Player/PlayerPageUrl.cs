using Microsoft.AspNetCore.Mvc;

namespace BBStats.Pages.Player;

public static class PlayerPageUrl
{
    public static string? For(IUrlHelper url, string steamId, string character, int page = 1)
    {
        if (page <= 1)
        {
            return url.Page("/Player/Index", new { steamId, character });
        }

        return url.Page("/Player/Index", new { steamId, character, matchPage = page });
    }
}
