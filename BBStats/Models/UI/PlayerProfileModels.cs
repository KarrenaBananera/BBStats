namespace BBStats.Models.UI;

public record CharacterSidebarItem(
    string Slug,
    string DisplayName,
    int Games,
    int Rating,
    bool IsActive);

public record GameResultRow(
	int Number,
	string Outcome,
	string OutcomeCss,
	DateTime PlayedAtUtc,
	string RatingDelta,
	string RatingDeltaCss,
	string? ReplayOpenUrl = null,
	string? ReplayDownloadUrl = null);

public record MatchSeriesViewModel(
    DateTime PlayedAtUtc,
    string OpponentName,
    string OpponentSteamId,
    string OpponentCharacterSlug,
    string OpponentCharacterDisplay,
    string SeriesScore,
    string SeriesScoreCss,
    string RatingDelta,
    string RatingDeltaCss,
    IReadOnlyList<GameResultRow> Games,
    IReadOnlyList<string> ReplayDownloadUrls);

public class PlayerProfilePageViewModel
{
    public string PlayerName { get; init; } = "";
    public string SteamId { get; init; } = "";
    public string CharacterSlug { get; init; } = "";
    public string CharacterDisplayName { get; init; } = "";
    public int OverallRank { get; init; }
    public int CharacterRank { get; init; }
    public int Rating { get; init; }
    public int RatingDeviation { get; init; }
    public IReadOnlyList<CharacterSidebarItem> Characters { get; init; } = [];
    public IReadOnlyList<MatchSeriesViewModel> Series { get; init; } = [];
    public int CurrentPage { get; init; } = 1;
    public int TotalPages { get; init; } = 1;
    public bool HasMatches => Series.Count > 0;
    public string? EmptyMessage { get; init; }
}
