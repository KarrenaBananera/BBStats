namespace BBStats.Data.Entites;

public class PlayerCharacterStat
{
	public Int64 PlayerId { get; set; }
	public int CharacterId { get; set; }
	public int Wins { get; set; }
	public int Losses { get; set; }
	public double CurrentRating { get; set; }
	public double MaxRating { get; set; }
	public double RatingDeviation { get; set; }	
	public double Volatility { get; set; }
	public DateTime LastGame { get; set; }
	public Player Player { get; set; }
	public Character Character { get; set; }
	public List<PlayerGame> Games { get ; set; }

}
