namespace BBStats.Models.UI;

public record CharacterSidebarItem(
    string Slug,
    string DisplayName,
    int Games,
    int Rating,
    int RatingDeviation,
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

public record MostPlayedOpponentRow(
	string OpponentName,
	string OpponentSteamId,
	string OpponentCharacterSlug,
	int Games,
	double WinratePercent,
	string WinrateCss);

public record PlayerCharacterMatchupRow(
	string CharacterName,
	string CharacterSlug,
	int Games,
	double WinratePercent,
	string WinrateCss);

public class PlayerCharacterStatsViewModel
{
	public IReadOnlyList<MostPlayedOpponentRow> MostPlayedOpponents { get; init; } = [];
	public IReadOnlyList<PlayerCharacterMatchupRow> CharacterMatchups { get; init; } = [];
}

public sealed class MatchSeriesFilter
{
	public const int MaxOpponentNameLength = 40;

	public static MatchSeriesFilter None { get; } = new();

	public int? MinOpponentElo { get; init; }
	public int? MaxOpponentElo { get; init; }
	public string? OpponentName { get; init; }
	public string? OpponentCharacterSlug { get; init; }

	public bool HasEloBounds => MinOpponentElo.HasValue || MaxOpponentElo.HasValue;
	public bool HasOpponentName => !string.IsNullOrEmpty(OpponentName);
	public bool HasOpponentCharacter => !string.IsNullOrEmpty(OpponentCharacterSlug);
	public bool IsActive => HasEloBounds || HasOpponentName || HasOpponentCharacter;

	public MatchSeriesFilter Normalize()
	{
		var name = string.IsNullOrWhiteSpace(OpponentName) ? null : OpponentName.Trim();
		if (name is { Length: > MaxOpponentNameLength })
		{
			name = name[..MaxOpponentNameLength];
		}

		var slug = string.IsNullOrWhiteSpace(OpponentCharacterSlug)
			? null
			: OpponentCharacterSlug.Trim().ToLowerInvariant();

		return new MatchSeriesFilter
		{
			MinOpponentElo = MinOpponentElo,
			MaxOpponentElo = MaxOpponentElo,
			OpponentName = name,
			OpponentCharacterSlug = slug
		};
	}
}

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
    public int Wins { get; init; }
    public int Losses { get; init; }
    public double WinratePercent { get; init; }
    public string WinrateCss { get; init; } = "text-muted";
    public IReadOnlyList<CharacterSidebarItem> Characters { get; init; } = [];
    public IReadOnlyList<MatchSeriesViewModel> Series { get; init; } = [];
    public int CurrentPage { get; init; } = 1;
    public int TotalPages { get; init; } = 1;
    public bool HasMatches => Series.Count > 0;
    public string? EmptyMessage { get; init; }
    public bool IsIgnored { get; set; }
    public MatchSeriesFilter SeriesFilter { get; init; } = MatchSeriesFilter.None;
}
