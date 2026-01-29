using BBStats.Services;

namespace BBStats.Data;

public interface IGamesRepository
{
	public void AddGame(GameDTO game);
}
