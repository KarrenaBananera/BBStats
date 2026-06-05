using System.Globalization;
using BBStats.Models.UI;

namespace BBStats.Pages.Player;

public static class PlayerProfileMock
{
    private static readonly IReadOnlyList<(string Slug, string Name, int Games, int Rating, int CharRank)> DefaultCharacters =
    [
        ("ragna", "Ragna", 120, 1850, 15),
        ("jin", "Jin", 45, 1720, 28),
        ("noel", "Noel", 18, 1580, 51),
    ];

    public static PlayerProfilePageViewModel GetProfile(string steamId, string characterSlug, int page)
    {
        var slug = characterSlug.ToLowerInvariant();
        var charEntry = DefaultCharacters.FirstOrDefault(c => c.Slug == slug);
        if (charEntry == default)
        {
            charEntry = DefaultCharacters[0];
        }
        var displayName = charEntry.Name;

        var characters = DefaultCharacters.Select(c => new CharacterSidebarItem(
            c.Slug,
            c.Name,
            c.Games,
            c.Rating,
            c.Slug == slug)).ToList();

        var (series, totalPages, emptyMessage) = GetSeriesForPage(slug, page);

        return new PlayerProfilePageViewModel
        {
            PlayerName = "Player123",
            SteamId = steamId,
            CharacterSlug = slug,
            CharacterDisplayName = displayName,
            OverallRank = 42,
            CharacterRank = charEntry.CharRank,
            Rating = charEntry.Rating,
            Characters = characters,
            Series = series,
            CurrentPage = page,
            TotalPages = totalPages,
            EmptyMessage = emptyMessage,
        };
    }

    private static (IReadOnlyList<MatchSeriesViewModel> Series, int TotalPages, string? EmptyMessage)
        GetSeriesForPage(string characterSlug, int page)
    {
        if (characterSlug == "noel")
        {
            return ([], 1, "No recorded series for this character (mockup).");
        }

        if (characterSlug == "jin")
        {
            return page switch
            {
                1 => ([CreateSeries(
                    "30.05.2026 16:00",
                    "RagnaPlayer",
                    "76561198011111111",
                    "ragna",
                    "Ragna",
                    "2 : 1",
                    "text-success",
                    "+10",
                    "text-success",
                    [("Loss", "text-danger"), ("Win", "text-success"), ("Win", "text-success")])], 1, null),
                _ => ([], 1, null),
            };
        }

        return page switch
        {
            1 => (Page1RagnaSeries(), 4, null),
            2 => ([
                CreateSeries("28.05.2026 19:45", "BB_Master", "98761198000000001", "jin", "Jin",
                    "2 : 0", "text-success", "+15", "text-success",
                    [("Win", "text-success"), ("Win", "text-success")]),
                CreateSeries("27.05.2026 12:20", "RagnaPlayer", "76561198011111111", "ragna", "Ragna",
                    "1 : 1", "text-warning", "0", "text-muted",
                    [("Win", "text-success"), ("Loss", "text-danger")]),
            ], 4, null),
            3 => ([
                CreateSeries("20.05.2026 10:00", "BB_Master", "98761198000000001", "jin", "Jin",
                    "1 : 2", "text-danger", "−9", "text-danger",
                    [("Win", "text-success"), ("Loss", "text-danger"), ("Loss", "text-danger")]),
            ], 4, null),
            4 => ([], 4, "Older series (mockup, page 4)."),
            _ => ([], 4, null),
        };
    }

    private static IReadOnlyList<MatchSeriesViewModel> Page1RagnaSeries() =>
    [
        CreateSeries("03.06.2026 18:30", "BB_Master", "98761198000000001", "jin", "Jin",
            "2 : 1", "text-success", "+12", "text-success",
            [("Win", "text-success"), ("Loss", "text-danger"), ("Win", "text-success")]),
        CreateSeries("02.06.2026 21:15", "RagnaPlayer", "76561198011111111", "ragna", "Ragna",
            "0 : 2", "text-danger", "−18", "text-danger",
            [("Loss", "text-danger"), ("Loss", "text-danger")]),
        CreateSeries("01.06.2026 14:00", "BB_Master", "98761198000000001", "jin", "Jin",
            "3 : 2", "text-success", "+8", "text-success",
            [("Win", "text-success"), ("Win", "text-success"), ("Loss", "text-danger"),
                ("Loss", "text-danger"), ("Win", "text-success")]),
    ];

    private static DateTime ParseMockUtc(string value) =>
        DateTime.SpecifyKind(
            DateTime.ParseExact(value, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
            DateTimeKind.Utc);

    private static MatchSeriesViewModel CreateSeries(
        string dateTime,
        string opponentName,
        string opponentSteamId,
        string opponentCharSlug,
        string opponentCharDisplay,
        string score,
        string scoreCss,
        string ratingDelta,
        string ratingDeltaCss,
        IReadOnlyList<(string Outcome, string Css)> games) =>
        CreateSeries(
            ParseMockUtc(dateTime),
            opponentName,
            opponentSteamId,
            opponentCharSlug,
            opponentCharDisplay,
            score,
            scoreCss,
            ratingDelta,
            ratingDeltaCss,
            games);

    private static MatchSeriesViewModel CreateSeries(
        DateTime playedAtUtc,
        string opponentName,
        string opponentSteamId,
        string opponentCharSlug,
        string opponentCharDisplay,
        string score,
        string scoreCss,
        string ratingDelta,
        string ratingDeltaCss,
        IReadOnlyList<(string Outcome, string Css)> games)
    {
        var rows = games
            .Select((g, i) => new GameResultRow(i + 1, g.Outcome, g.Css, playedAtUtc, "0", "text-muted"))
            .ToList();

        var downloadUrls = rows
            .Select(row => row.ReplayDownloadUrl)
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Cast<string>()
            .ToList();

        return new MatchSeriesViewModel(
            playedAtUtc,
            opponentName,
            opponentSteamId,
            opponentCharSlug,
            opponentCharDisplay,
            score,
            scoreCss,
            ratingDelta,
            ratingDeltaCss,
            rows,
            downloadUrls);
    }
}
