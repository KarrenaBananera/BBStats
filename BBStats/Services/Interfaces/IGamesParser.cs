using BBStats.Services.Implementation;

namespace BBStats.Services.Interfaces
{
	public interface IGamesParser
	{
		List<GameDTO> Parse(string data);
	}
}