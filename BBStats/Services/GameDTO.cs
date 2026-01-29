namespace BBStats.Services;

public record class GameDTO
{
	public string PlayerA { get; init; }
	public string PlayerB { get; init; }
	public long PlayerAId { get; init; }
	public long PlayerBId { get; init; }
	public string GameUrl { get; init; }
	public DateTime PlayedAt { get; init; }
	public bool IsPlayerAWin { get; init; }
	public int CharacterAId { get; init; }
	public int CharacterBId { get; init; }

}
