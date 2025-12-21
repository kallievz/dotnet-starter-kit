using Microsoft.AspNetCore.Identity;
using FSH.Modules.Identity.Features.v1.Users.PasswordHistory;

namespace FSH.Modules.Identity.Features.v1.Users;

public class FshUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Uri? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }

    public string? ObjectId { get; set; }

    /// <summary>Timestamp when the user last changed their password</summary>
    public DateTime LastPasswordChangeDate { get; set; } = DateTime.UtcNow;

    // Navigation property for password history
    public virtual ICollection<global::FSH.Modules.Identity.Features.v1.Users.PasswordHistory.PasswordHistory> PasswordHistories { get; set; } = new List<global::FSH.Modules.Identity.Features.v1.Users.PasswordHistory.PasswordHistory>();
}