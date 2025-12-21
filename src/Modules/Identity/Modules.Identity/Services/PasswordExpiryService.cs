using FSH.Modules.Identity.Data;
using FSH.Modules.Identity.Features.v1.Users;
using Microsoft.Extensions.Options;

namespace FSH.Modules.Identity.Services;

public interface IPasswordExpiryService
{
    /// <summary>Check if a user's password has expired</summary>
    bool IsPasswordExpired(FshUser user);

    /// <summary>Get the number of days until password expires (-1 if already expired)</summary>
    int GetDaysUntilExpiry(FshUser user);

    /// <summary>Check if password is expiring soon (within warning period)</summary>
    bool IsPasswordExpiringWithinWarningPeriod(FshUser user);

    /// <summary>Get expiry status with detailed information</summary>
    PasswordExpiryStatus GetPasswordExpiryStatus(FshUser user);

    /// <summary>Update the last password change date for a user</summary>
    void UpdateLastPasswordChangeDate(FshUser user);
}

public class PasswordExpiryStatus
{
    public bool IsExpired { get; set; }
    public bool IsExpiringWithinWarningPeriod { get; set; }
    public int DaysUntilExpiry { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public string Status
    {
        get
        {
            if (IsExpired)
                return "Expired";
            if (IsExpiringWithinWarningPeriod)
                return "Expiring Soon";
            return "Valid";
        }
    }
}

internal sealed class PasswordExpiryService : IPasswordExpiryService
{
    private readonly PasswordPolicyOptions _passwordPolicyOptions;

    public PasswordExpiryService(IOptions<PasswordPolicyOptions> passwordPolicyOptions)
    {
        _passwordPolicyOptions = passwordPolicyOptions.Value;
    }

    public bool IsPasswordExpired(FshUser user)
    {
        if (!_passwordPolicyOptions.EnforcePasswordExpiry)
        {
            return false;
        }

        var expiryDate = user.LastPasswordChangeDate.AddDays(_passwordPolicyOptions.PasswordExpiryDays);
        return DateTime.UtcNow > expiryDate;
    }

    public int GetDaysUntilExpiry(FshUser user)
    {
        if (!_passwordPolicyOptions.EnforcePasswordExpiry)
        {
            return int.MaxValue;
        }

        var expiryDate = user.LastPasswordChangeDate.AddDays(_passwordPolicyOptions.PasswordExpiryDays);
        var daysUntilExpiry = (int)(expiryDate - DateTime.UtcNow).TotalDays;
        return daysUntilExpiry;
    }

    public bool IsPasswordExpiringWithinWarningPeriod(FshUser user)
    {
        if (!_passwordPolicyOptions.EnforcePasswordExpiry)
        {
            return false;
        }

        var daysUntilExpiry = GetDaysUntilExpiry(user);
        return daysUntilExpiry >= 0 && daysUntilExpiry <= _passwordPolicyOptions.PasswordExpiryWarningDays;
    }

    public PasswordExpiryStatus GetPasswordExpiryStatus(FshUser user)
    {
        var expiryDate = user.LastPasswordChangeDate.AddDays(_passwordPolicyOptions.PasswordExpiryDays);
        var daysUntilExpiry = GetDaysUntilExpiry(user);
        var isExpired = IsPasswordExpired(user);
        var isExpiringWithinWarningPeriod = IsPasswordExpiringWithinWarningPeriod(user);

        return new PasswordExpiryStatus
        {
            IsExpired = isExpired,
            IsExpiringWithinWarningPeriod = isExpiringWithinWarningPeriod,
            DaysUntilExpiry = daysUntilExpiry,
            ExpiryDate = _passwordPolicyOptions.EnforcePasswordExpiry ? expiryDate : null
        };
    }

    public void UpdateLastPasswordChangeDate(FshUser user)
    {
        user.LastPasswordChangeDate = DateTime.UtcNow;
    }
}
