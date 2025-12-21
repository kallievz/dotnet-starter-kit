namespace FSH.Modules.Identity.Features.v1.Users.PasswordHistory;

public class PasswordHistory
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual FshUser? User { get; set; }
}
