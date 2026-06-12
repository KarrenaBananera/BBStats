namespace BBStats.Data.Entites;

public class IgnoredPlayer
{
    public Int64 PlayerId { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}