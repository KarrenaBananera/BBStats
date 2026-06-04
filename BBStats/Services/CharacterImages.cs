using System.Collections.ObjectModel;

namespace BBStats.Services;

public static class CharacterImages
{
	private static readonly IReadOnlyDictionary<string, string> StatsIconFileNames =
		new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			["Ragna"] = "BBCF_Ragna_the_Bloodedge_Icon.png",
			["Jin"] = "BBCF_Jin_Kisaragi_Icon.png",
			["Noel"] = "BBCF_Noel_Vermillion_Icon.png",
			["Rachel"] = "BBCF_Rachel_Alucard_Icon.png",
			["Taokaka"] = "BBCF_Taokaka_Icon.png",
			["Tager"] = "BBCF_Iron_Tager_Icon.png",
			["Litchi"] = "BBCF_Litchi_Faye_Ling_Icon.png",
			["Arakune"] = "BBCF_Arakune_Icon.png",
			["Bang"] = "BBCF_Bang_Shishigami_Icon.png",
			["Carl"] = "BBCF_Carl_Clover_Icon.png",
			["Hakumen"] = "BBCF_Hakumen_Icon.png",
			["Nu"] = "BBCF_Nu-13_Icon.png",
			["Tsubaki"] = "BBCF_Tsubaki_Yayoi_Icon.png",
			["Hazama"] = "BBCF_Hazama_Icon.png",
			["Mu"] = "BBCF_Mu-12_Icon.png",
			["Makoto"] = "BBCF_Makoto_Nanaya_Icon.png",
			["Valkenhayn"] = "BBCF_Valkenhayn_R._Hellsing_Icon.png",
			["Platinum"] = "BBCF_Platinum_the_Trinity_Icon.png",
			["Relius"] = "BBCF_Relius_Clover_Icon.png",
			["Izayoi"] = "BBCF_Izayoi_Icon.png",
			["Amane"] = "BBCF_Amane_Nishiki_Icon.png",
			["Bullet"] = "BBCF_Bullet_Icon.png",
			["Azrael"] = "BBCF_Azrael_Icon.png",
			["Kagura"] = "BBCF_Kagura_Mutsuki_Icon.png",
			["Kokonoe"] = "BBCF_Kokonoe_Icon.png",
			["Terumi"] = "BBCF_Yuuki_Terumi_Icon.png",
			["Celica"] = "BBCF_Celica_A._Mercury_Icon.png",
			["Lambda"] = "BBCF_Lambda-11_Icon.png",
			["Hibiki"] = "BBCF_Hibiki_Kohaku_Icon.png",
			["Nine"] = "BBCF_Nine_the_Phantom_Icon.png",
			["Naoto"] = "BBCF_Naoto_Kurogane_Icon.png",
			["Izanami"] = "BBCF_Izanami_Icon.png",
			["Susanoo"] = "BBCF_Susano'o_Icon.png",
			["Es"] = "BBCF_Es_Icon.png",
			["Mai"] = "BBCF_Mai_Natsume_Icon.png",
			["Jubei"] = "BBCF_Jubei_Icon.png",
		});

	public static string GetPortraitUrl(string characterName) =>
		$"/data/characters/{characterName}.png";

	public static string GetSmallIconUrl(string characterName) =>
		$"/data/characters/smallIcons/{characterName}.png";

	public static string GetStatsIconUrl(string characterName) =>
		StatsIconFileNames.TryGetValue(characterName, out var fileName)
			? $"/data/characters/stats icons/{fileName}"
			: GetPortraitUrl(characterName);
}
