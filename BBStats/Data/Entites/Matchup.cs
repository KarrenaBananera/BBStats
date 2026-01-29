using System.ComponentModel.DataAnnotations.Schema;

namespace BBStats.Data.Entites;

public class Matchup
{
	public int CharacterAId { get; set; }
	public int CharacterBId { get; set; }
	public int TotalGames { get; set; }
	public int WinsA { get; set; }
	public int WinsB { get; set; }
	public Character CharacterA { get; set; }
	public Character CharacterB { get; set; }
}