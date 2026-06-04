using System.Collections.ObjectModel;

namespace BBStats.Services;

public static class CharacterImages
{
	private static readonly IReadOnlyDictionary<string, string> IconUrls =
		new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			["Ragna"] = "https://www.dustloop.com/wiki/images/1/1a/BBCF_Ragna_the_Bloodedge_Icon.png",
			["Jin"] = "https://www.dustloop.com/wiki/images/1/1e/BBCF_Jin_Kisaragi_Icon.png",
			["Noel"] = "https://www.dustloop.com/wiki/images/e/e8/BBCF_Noel_Vermillion_Icon.png",
			["Rachel"] = "https://www.dustloop.com/wiki/images/6/63/BBCF_Rachel_Alucard_Icon.png",
			["Taokaka"] = "https://www.dustloop.com/wiki/images/8/80/BBCF_Taokaka_Icon.png",
			["Tager"] = "https://www.dustloop.com/wiki/images/6/6c/BBCF_Iron_Tager_Icon.png",
			["Litchi"] = "https://www.dustloop.com/wiki/images/e/ed/BBCF_Litchi_Faye_Ling_Icon.png",
			["Arakune"] = "https://www.dustloop.com/wiki/images/d/df/BBCF_Arakune_Icon.png",
			["Bang"] = "https://www.dustloop.com/wiki/images/a/a5/BBCF_Bang_Shishigami_Icon.png",
			["Carl"] = "https://www.dustloop.com/wiki/images/1/1f/BBCF_Carl_Clover_Icon.png",
			["Hakumen"] = "https://www.dustloop.com/wiki/images/4/4a/BBCF_Hakumen_Icon.png",
			["Nu"] = "https://www.dustloop.com/wiki/images/1/10/BBCF_Nu-13_Icon.png",
			["Tsubaki"] = "https://www.dustloop.com/wiki/images/6/67/BBCF_Tsubaki_Yayoi_Icon.png",
			["Hazama"] = "https://www.dustloop.com/wiki/images/d/db/BBCF_Hazama_Icon.png",
			["Mu"] = "https://www.dustloop.com/wiki/images/b/b1/BBCF_Mu-12_Icon.png",
			["Makoto"] = "https://www.dustloop.com/wiki/images/6/6a/BBCF_Makoto_Nanaya_Icon.png",
			["Valkenhayn"] = "https://www.dustloop.com/wiki/images/6/68/BBCF_Valkenhayn_R._Hellsing_Icon.png",
			["Platinum"] = "https://www.dustloop.com/wiki/images/d/da/BBCF_Platinum_the_Trinity_Icon.png",
			["Relius"] = "https://www.dustloop.com/wiki/images/7/73/BBCF_Relius_Clover_Icon.png",
			["Izayoi"] = "https://www.dustloop.com/wiki/images/f/f2/BBCF_Izayoi_Icon.png",
			["Amane"] = "https://www.dustloop.com/wiki/images/8/86/BBCF_Amane_Nishiki_Icon.png",
			["Bullet"] = "https://www.dustloop.com/wiki/images/1/17/BBCF_Bullet_Icon.png",
			["Azrael"] = "https://www.dustloop.com/wiki/images/7/71/BBCF_Azrael_Icon.png",
			["Kagura"] = "https://www.dustloop.com/wiki/images/7/72/BBCF_Kagura_Mutsuki_Icon.png",
			["Kokonoe"] = "https://www.dustloop.com/wiki/images/2/2b/BBCF_Kokonoe_Icon.png",
			["Terumi"] = "https://www.dustloop.com/wiki/images/e/ee/BBCF_Yuuki_Terumi_Icon.png",
			["Celica"] = "https://www.dustloop.com/wiki/images/a/a1/BBCF_Celica_A._Mercury_Icon.png",
			["Lambda"] = "https://www.dustloop.com/wiki/images/2/28/BBCF_Lambda-11_Icon.png",
			["Hibiki"] = "https://www.dustloop.com/wiki/images/f/fb/BBCF_Hibiki_Kohaku_Icon.png",
			["Nine"] = "https://www.dustloop.com/wiki/images/1/1f/BBCF_Nine_the_Phantom_Icon.png",
			["Naoto"] = "https://www.dustloop.com/wiki/images/2/2e/BBCF_Naoto_Kurogane_Icon.png",
			["Izanami"] = "https://www.dustloop.com/wiki/images/1/13/BBCF_Izanami_Icon.png",
			["Susanoo"] = "https://www.dustloop.com/wiki/images/6/64/BBCF_Susano'o_Icon.png",
			["Es"] = "https://www.dustloop.com/wiki/images/a/a1/BBCF_Es_Icon.png",
			["Mai"] = "https://www.dustloop.com/wiki/images/a/a2/BBCF_Mai_Natsume_Icon.png",
			["Jubei"] = "https://www.dustloop.com/wiki/images/6/63/BBCF_Jubei_Icon.png",
		});

	public static string GetPortraitUrl(string characterName) =>
		$"/data/characters/{characterName}.png";

	public static string GetSmallIconUrl(string characterName) =>
		$"/data/characters/smallIcons/{characterName}.png";

	public static string GetIconUrl(string characterName) =>
		IconUrls.TryGetValue(characterName, out var url) ? url : GetPortraitUrl(characterName);
}
