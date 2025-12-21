using FluentValidation;
using FSH.Framework.Shared.Identity.Claims;
using FSH.Modules.Identity.Contracts.v1.Users.ChangePassword;
using FSH.Modules.Identity.Features.v1.Users;
using FSH.Modules.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace FSH.Modules.Identity.Features.v1.Users.ChangePassword;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    private readonly UserManager<FshUser> _userManager;
    private readonly IPasswordHistoryService _passwordHistoryService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChangePasswordValidator(
        UserManager<FshUser> userManager,
        IPasswordHistoryService passwordHistoryService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _passwordHistoryService = passwordHistoryService;
        _httpContextAccessor = httpContextAccessor;

        RuleFor(p => p.Password)
            .NotEmpty()
            .WithMessage("Current password is required.");

        RuleFor(p => p.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required.")
            .NotEqual(p => p.Password)
            .WithMessage("New password must be different from the current password.")
            .MustAsync(NotBeInPasswordHistoryAsync)
            .WithMessage("This password has been used recently. Please choose a different password.");

        RuleFor(p => p.ConfirmNewPassword)
            .Equal(p => p.NewPassword)
            .WithMessage("Passwords do not match.");
    }

    private async Task<bool> NotBeInPasswordHistoryAsync(string newPassword, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return true; // Let other validation handle unauthorized access
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return true; // Let other validation handle user not found
        }

        // Check if password is in history
        var isInHistory = await _passwordHistoryService.IsPasswordInHistoryAsync(user, newPassword, cancellationToken);
        return !isInHistory; // Return true if NOT in history (validation passes)
    }
}