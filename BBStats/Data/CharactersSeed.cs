using BBStats.Data.Entites;
using System.Collections.ObjectModel;

namespace BBStats.Data;

public static class CharactersSeed
{
	public static readonly Character[] All =
	{
		new() { Id = 1, Name = "Ragna" },
		new() { Id = 2, Name = "Jin" },
		new() { Id = 3, Name = "Noel" },
		new() { Id = 4, Name = "Rachel" },
		new() { Id = 5, Name = "Taokaka" },
		new() { Id = 6, Name = "Tager" },
		new() { Id = 7, Name = "Litchi" },
		new() { Id = 8, Name = "Arakune" },
		new() { Id = 9, Name = "Bang" },
		new() { Id = 10, Name = "Carl" },
		new() { Id = 11, Name = "Hakumen" },
		new() { Id = 12, Name = "Nu" },
		new() { Id = 13, Name = "Tsubaki" },
		new() { Id = 14, Name = "Hazama" },
		new() { Id = 15, Name = "Mu" },
		new() { Id = 16, Name = "Makoto" },
		new() { Id = 17, Name = "Valkenhayn" },
		new() { Id = 18, Name = "Platinum" },
		new() { Id = 19, Name = "Relius" },
		new() { Id = 20, Name = "Izayoi" },
		new() { Id = 21, Name = "Amane" },
		new() { Id = 22, Name = "Bullet" },
		new() { Id = 23, Name = "Azrael" },	
		new() { Id = 24, Name = "Kagura" },
		new() { Id = 25, Name = "Kokonoe" },
		new() { Id = 26, Name = "Terumi" },
		new() { Id = 27, Name = "Celica" },
		new() { Id = 28, Name = "Lambda" },
		new() { Id = 29, Name = "Hibiki" },
		new() { Id = 30, Name = "Nine" },
		new() { Id = 31, Name = "Naoto" },
		new() { Id = 32, Name = "Izanami" },
		new() { Id = 33, Name = "Susanoo" },
		new() { Id = 34, Name = "Es" },
		new() { Id = 35, Name = "Mai" },
		new() { Id = 36, Name = "Jubei" },
	};

	public static readonly ReadOnlyDictionary<string, int> CharactersNames;

	static CharactersSeed()
	{
		var dictonary = new Dictionary<string, int>();

		foreach (var character in All)	
		{
			dictonary.Add(character.Name.ToLower(), character.Id);
		}

		CharactersNames = new(dictonary);

	}
}
