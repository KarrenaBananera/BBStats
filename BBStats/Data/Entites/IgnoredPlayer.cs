namespace BBStats.Data.Entites;
// This entity is used to maintain a list of ignored players that should be excluded from all public queriess
public class IgnoredPlayer
{
    public Int64 PlayerId { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}