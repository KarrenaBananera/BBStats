namespace BBStats.Configuration;

public class HistoricalGamesFetcherOptions
{
	public const string SectionName = "HistoricalGamesFetcher";

	public string BaseUrl { get; set; } = string.Empty;

	public bool Enabled { get; set; } = true;

	public int FetchIntervalSeconds { get; set; } = 86400; // default 1 day
}
