using System.ComponentModel.DataAnnotations;

namespace BBStats.Data.Entites;

public class Character
{
	public int Id { get; set; }

	[Required]
	[MaxLength(20)]
	public string Name { get; set; }
}
