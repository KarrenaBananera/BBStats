using BBStats.Models.UI;

namespace BBStats.Services;

public interface IPlayerProfileService
{
	Task<PlayerProfileResult> GetProfileAsync(
		long playerId,
		string characterSlug,
		int pageNumber,
		bool includeIgnored,
		CancellationToken cancellationToken = default);
}

public sealed class PlayerProfileResult
{
	public PlayerProfilePageViewModel? Profile { get; init; }
	public string? RedirectCharacterSlug { get; init; }

	public static PlayerProfileResult Found(PlayerProfilePageViewModel profile) =>
		new() { Profile = profile };

	public static PlayerProfileResult RedirectToCharacter(string slug) =>
		new() { RedirectCharacterSlug = slug };

	public static PlayerProfileResult NotFound() => new();
}
