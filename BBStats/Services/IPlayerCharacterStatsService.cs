using BBStats.Models.UI;

namespace BBStats.Services;

public interface IPlayerCharacterStatsService
{
	Task<PlayerCharacterStatsViewModel?> GetStatsAsync(
		long playerId,
		string characterSlug,
		CancellationToken cancellationToken = default);
}
