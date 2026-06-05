namespace BBStats.Configuration;

public class GamesFetcherOptions
{
	public const string SectionName = "GamesFetcher";

	public string BaseUrl { get; set; } = string.Empty;

	public string UrlForFront { get; set; } = string.Empty;

	public bool Enabled { get; set; } = true;

	public int FetchIntervalSeconds { get; set; } = 60;
}
