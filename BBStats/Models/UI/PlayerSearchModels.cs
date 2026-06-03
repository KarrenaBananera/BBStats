namespace BBStats.Models.UI;

public record PlayerSearchResultItem(
	string SteamId,
	string PlayerName,
	string CharacterSlug);

public class PlayerSearchPageViewModel
{
	public string Query { get; init; } = "";
	public IReadOnlyList<PlayerSearchResultItem> Results { get; init; } = [];
	public string? Message { get; init; }
	public bool HasSearched { get; init; }
}
