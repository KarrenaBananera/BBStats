namespace BBStats.Models.UI;

public record TopPlayerRowViewModel(
	int Rank,
	string PlayerName,
	string SteamId,
	string CharacterName,
	string CharacterSlug,
	int Rating);

public class TopPageViewModel
{
	public const int PageSize = 100;

	public int PageNumber { get; init; } = 1;
	public int TotalPages { get; init; } = 1;
	public int TotalEntries { get; init; }
	public IReadOnlyList<TopPlayerRowViewModel> Rows { get; init; } = [];

	public bool HasPreviousPage => PageNumber > 1;
	public bool HasNextPage => PageNumber < TotalPages;
}
