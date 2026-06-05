using System.ComponentModel.DataAnnotations;

namespace BBStats.Data.Entites;

public class Game
{
	public int Id { get; set; }

	[MaxLength(40)]
	public string PlayerA { get; set; }

	[MaxLength(40)]
	public string PlayerB { get; set; }
	public Int64 PlayerAId { get; set; }
	public Int64 PlayerBId { get; set; }	
	[MaxLength(64)]
	public string ReplayId { get; set; }	
	public DateTime PlayedAt { get; set; }
	public bool IsPlayerAWin { get; set; }

	public int CharacterAId { get; set; }

	public Character CharacterA { get; set; }

	public int CharacterBId { get; set; }

	public Character CharacterB { get; set; }
}
