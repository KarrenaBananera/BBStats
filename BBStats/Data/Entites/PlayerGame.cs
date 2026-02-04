namespace BBStats.Data.Entites;

public class PlayerGame
{
	public int GameId { get; set; }
	public Int64 PlayerId { get; set; }
	public double EloBefore { get; set; }
	public double EloAfter { get; set; }
	public Game Game { get; set; }
	public Player Player { get; set; }
}
