namespace BBStats.Services
{
	public interface IGamesParser
	{
		List<GameDTO> Parse(string data);
	}
}