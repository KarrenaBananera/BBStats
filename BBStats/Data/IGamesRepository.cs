using BBStats.Services.Implementation;

namespace BBStats.Data;

public interface IGamesRepository
{
	public Task<bool> AddGameAsync(GameDTO game);

}
