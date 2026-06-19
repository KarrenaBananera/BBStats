using BBStats.Models.UI;

namespace BBStats.Services;

public interface IPlayerSearchService
{
	Task<bool> PlayerExistsAsync(long steamId, CancellationToken cancellationToken = default);

	Task<string> GetDefaultCharacterSlugAsync(long steamId, CancellationToken cancellationToken = default);

	Task<IReadOnlyList<PlayerSearchResultItem>> SearchByNameAsync(
		string query,
		bool includeIgnored, 
		CancellationToken cancellationToken = default);
}
