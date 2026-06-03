namespace BBStats.Models.UI;

public record MatchupStatViewModel(
	string Name,
	string ImageUrl,
	double Frequency,
	int Matches,
	double Winrate,
	string WinrateCss);

public class CharacterStatisticsViewModel
{
	public string CharacterSlug { get; init; } = "";
	public string DisplayName { get; init; } = "";
	public string PortraitImageUrl { get; init; } = "";
	public double UsagePercent { get; init; }
	public double WinratePercent { get; init; }
	public int TotalMatches { get; init; }
	public IReadOnlyList<MatchupStatViewModel> Matchups { get; init; } = [];
}
