namespace BBStats.Data.Entites;
// This entity is used to maintain a list of ignored players to prevent them from being added back to the database after deleting them
public class IgnoredPlayer
{
    public Int64 PlayerId { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}