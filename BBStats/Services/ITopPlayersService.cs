using BBStats.Models.UI;

namespace BBStats.Services;

public interface ITopPlayersService
{
	Task<TopPageViewModel> GetPageAsync(
		int pageNumber,
		int? characterId = null,
		CancellationToken cancellationToken = default);
}
