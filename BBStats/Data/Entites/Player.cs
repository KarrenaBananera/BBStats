using System.ComponentModel.DataAnnotations;

namespace BBStats.Data.Entites;

public class Player
{
	public Int64 Id { get; set; }

	[MaxLength(40)]
	public string Name { get; set; }

	public List<PlayerCharacterStat> CharactersStat { get; set; }
}
