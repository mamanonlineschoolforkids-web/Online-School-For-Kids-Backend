using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task SendVerificationEmailAsync(string to, string userName, DateTime expiry, string verificationLink, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string to, string userName, DateTime expiry, string resetLink, CancellationToken cancellationToken = default);
    Task SendPasswordChangedEmailAsync(string email);
    Task SendWelcomeEmailAsync(string to, string userName, CancellationToken cancellationToken = default);

    /// Send parent-child link invitation
    Task SendParentLinkInvitationAsync(string childEmail, string childName, string parentName, string inviteToken, CancellationToken cancellationToken = default);

    /// Notify child that parent has linked their account
    Task SendParentLinkedNotificationAsync(string childEmail, string childName, string parentName, CancellationToken cancellationToken = default);
}