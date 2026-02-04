namespace BBStats.Data.Entites;

public class PlayerGame
{
	public int GameId { get; set; }
	public Int64 PlayerId { get; set; }
	public int EloBefore { get; set; }
	public int EloAfter { get; set; }
	public Game Game { get; set; }
}
