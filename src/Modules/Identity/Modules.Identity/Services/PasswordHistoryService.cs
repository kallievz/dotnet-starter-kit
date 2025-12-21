using FSH.Modules.Identity.Data;
using FSH.Modules.Identity.Features.v1.Users;
using FSH.Modules.Identity.Features.v1.Users.PasswordHistory;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FSH.Modules.Identity.Services;

public interface IPasswordHistoryService
{
    Task<bool> IsPasswordInHistoryAsync(FshUser user, string newPassword, CancellationToken cancellationToken = default);
    Task SavePasswordHistoryAsync(FshUser user, CancellationToken cancellationToken = default);
    Task CleanupOldPasswordHistoryAsync(string userId, CancellationToken cancellationToken = default);
}

internal sealed class PasswordHistoryService : IPasswordHistoryService
{
    private readonly IdentityDbContext _db;
    private readonly UserManager<FshUser> _userManager;
    private readonly PasswordPolicyOptions _passwordPolicyOptions;

    public PasswordHistoryService(
        IdentityDbContext db,
        UserManager<FshUser> userManager,
        IOptions<PasswordPolicyOptions> passwordPolicyOptions)
    {
        _db = db;
        _userManager = userManager;
        _passwordPolicyOptions = passwordPolicyOptions.Value;
    }

    public async Task<bool> IsPasswordInHistoryAsync(FshUser user, string newPassword, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(newPassword);

        // Get the last N passwords from history (where N = PasswordHistoryCount)
        var passwordHistoryCount = _passwordPolicyOptions.PasswordHistoryCount;
        if (passwordHistoryCount <= 0)
        {
            return false; // Password history check disabled
        }

        var recentPasswordHashes = await _db.Set<PasswordHistory>()
            .Where(ph => ph.UserId == user.Id)
            .OrderByDescending(ph => ph.CreatedAt)
            .Take(passwordHistoryCount)
            .Select(ph => ph.PasswordHash)
            .ToListAsync(cancellationToken);

        // Check if the new password matches any recent password
        foreach (var passwordHash in recentPasswordHashes)
        {
            var passwordHasher = _userManager.PasswordHasher;
            var result = passwordHasher.VerifyHashedPassword(user, passwordHash, newPassword);

            if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                return true; // Password is in history
            }
        }

        return false; // Password is not in history
    }

    public async Task SavePasswordHistoryAsync(FshUser user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var passwordHistoryEntry = new PasswordHistory
        {
            UserId = user.Id,
            PasswordHash = user.PasswordHash!,
            CreatedAt = DateTime.UtcNow
        };

        _db.Set<PasswordHistory>().Add(passwordHistoryEntry);
        await _db.SaveChangesAsync(cancellationToken);

        // Clean up old password history entries
        await CleanupOldPasswordHistoryAsync(user.Id, cancellationToken);
    }

    public async Task CleanupOldPasswordHistoryAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userId);

        var passwordHistoryCount = _passwordPolicyOptions.PasswordHistoryCount;
        if (passwordHistoryCount <= 0)
        {
            return; // Password history disabled
        }

        // Get all password history entries for the user, ordered by most recent
        var allPasswordHistories = await _db.Set<PasswordHistory>()
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedAt)
            .ToListAsync(cancellationToken);

        // Keep only the configured number of passwords
        if (allPasswordHistories.Count > passwordHistoryCount)
        {
            var oldPasswordHistories = allPasswordHistories
                .Skip(passwordHistoryCount)
                .ToList();

            _db.Set<PasswordHistory>().RemoveRange(oldPasswordHistories);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
