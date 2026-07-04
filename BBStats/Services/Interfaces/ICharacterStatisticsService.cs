using BBStats.Models.UI;

namespace BBStats.Services.Interfaces;

public interface ICharacterStatisticsService
{
	Task<CharacterStatisticsViewModel?> GetStatisticsAsync(int characterId, CancellationToken cancellationToken = default);
}
