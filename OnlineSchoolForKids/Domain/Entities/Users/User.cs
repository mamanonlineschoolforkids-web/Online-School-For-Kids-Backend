using Domain.Enums.Users;

namespace Domain.Entities.Users;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }

    public string? PasswordHash { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    public UserRole Role { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime DateOfBirth { get; set; }
    public string Country { get; set; }
    public string? Phone { get; set; }
    public string? Bio { get; set; }

    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;
    public bool IsFirstLogin { get; set; } = true;
    public string? GoogleId { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public NotificationPreferences? NotificationPreferences { get; set; }
    public List<PaymentMethod>? PaymentMethods { get; set; }
    public List<SocialLink>? SocialLinks { get; set; }



    // creator , specialist register data
    public string? PortfolioUrl { get; set; }
    public string? CvLink { get; set; }

    // student
    public string? ParentId { get; set; }

    // Parent
    public List<string>? ChildrenIds { get; set; }
    public List<string>? ChildInvitaions { get; set; }
    public Dictionary<string, NotificationPreferences>? ChildNotificationPreferences { get; set; }

    // Parent , Student
    public string? LearningGoals { get; set; }
    public List<string>? EnrolledCourseIds { get; set; }
    public List<string>? AchievementIds { get; set; }
    public int? TotalHoursLearned { get; set; } = 0;
    public int? Points { get; set; } = 0; // TODO:: in frontend

    
    // Content Creator , Specialist

    public List<string>? ExpertiseTags { get; set; }
    public int? TotalStudents { get; set; }
    public decimal? TotalRevenue { get; set; }
    public double? AverageRating { get; set; }
    public int? ReviewsCount { get; set; }
    public int? StudentsCount { get; set; }
    public int? CoursesCount { get; set; }



    public List<string>? CreatedCourseIds { get; set; }
    public List<WorkExperience>? WorkExperiences { get; set; }
    public List<Certification>? Certifications { get; set; }


    public bool? IsVerifiedCreator { get; set; } //TODO:: remove from front
 

    // Specialist
    public string? ProfessionalTitle { get; set; }
    public int? YearsOfExperience { get; set; }
    public List<AvailabilitySlot>? Availability { get; set; }
    public decimal? HourlyRate { get; set; }
    public SessionRates? SessionRates { get; set; }

    
    // Admin
    public bool? IsSuperAdmin { get; set; }

    public bool? TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    public bool? LoginNotifications { get; set; }
    public bool? SuspiciousActivityAlerts { get; set; }

    public List<ActivityLogEntry>? ActivityLog { get; set; }
}

public class NotificationPreferences
{
    public bool? WeeklyReports { get; set; }
    public bool? Messages { get; set; }
    public bool? CommentReplies { get; set; }



    // Student , parent

    public bool? ProgressUpdates { get; set; }
    public bool? AchievementAlerts { get; set; }
    public bool? PaymentReminders { get; set; } //TODO:: for subscribtions

    // Content Creator
    public bool? CourseEnrollments { get; set; }

    // specialist

    public bool? NewSessionBooking { get; set; }
    public bool? SessionCancellation { get; set; }
    public bool? SessionReminder { get; set; }


    // Content Creator , specialist
    public bool? ReviewNotifications { get; set; }
    public bool? PayoutAlerts { get; set; } // period payouts


    // admin

    public bool? AccountLogin { get; set; }
    public bool? SuspiciousActivity { get; set; }

}

public class SocialLink
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}


public class Certification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public int Year { get; set; }

    public string? DocumentUrl { get; set; }
}

public class AvailabilitySlot
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Day { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}

public class SessionRates
{
    public decimal ThirtyMinSession { get; set; }
    public decimal SixtyMinSession { get; set; }
    public decimal PlatformFeePercentage { get; set; } = 15;
}

public class ActivityLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? TargetType { get; set; }  
    public string? IpAddress { get; set; } 
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class WorkExperience
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Place { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public bool IsCurrentRole { get; set; }
}

public class PaymentMethod
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public PaymentMethodType Type { get; set; }

    public bool IsDefault { get; set; }

    // ── Card ──────────────────────────────────────────────────────────────────
    public string? Last4 { get; set; }
    public string? Brand { get; set; }          // Visa / Mastercard
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }
    /// <summary>Token from payment gateway (Paymob card token). Never store raw CVV.</summary>
    public string? CardToken { get; set; }
    public string? CardholderName { get; set; }

    // ── Vodafone Cash ─────────────────────────────────────────────────────────
    public string? VodafoneNumber { get; set; }

    // ── Instapay ─────────────────────────────────────────────────────────────
    public string? InstapayId { get; set; }     // phone or email registered with Instapay

    // ── Fawry ────────────────────────────────────────────────────────────────
    public string? FawryReferenceNumber { get; set; }

    // ── Bank Account ─────────────────────────────────────────────────────────
    public string? AccountHolderName { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? IBAN { get; set; }

    // ── Timestamps ───────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Computed display string (used by front-end) ───────────────────────────
    public string DisplayInfo => Type switch
    {
        PaymentMethodType.Card =>
            Last4 is not null ? $"{Brand ?? "Card"} •••• {Last4}" : "Card",
        PaymentMethodType.VodafoneCash =>
            VodafoneNumber is not null ? MaskPhone(VodafoneNumber) : "Vodafone Cash",
        PaymentMethodType.Instapay =>
            InstapayId is not null ? MaskIdentifier(InstapayId) : "Instapay",
        PaymentMethodType.Fawry =>
            FawryReferenceNumber is not null ? $"Fawry #{FawryReferenceNumber}" : "Fawry",
        PaymentMethodType.BankAccount =>
            AccountNumber is not null
                ? $"{BankName ?? "Bank"} •••• {AccountNumber[^Math.Min(4, AccountNumber.Length)..]}"
                : "Bank Account",
        _ => "Payment Method"
    };

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static string MaskPhone(string phone)
    {
        if (phone.Length < 4) return phone;
        return $"****{phone[^4..]}";
    }

    private static string MaskIdentifier(string id)
    {
        if (id.Contains('@'))
        {
            // email — mask local part
            var parts = id.Split('@');
            var local = parts[0];
            var masked = local.Length <= 2 ? "**" : $"{local[0]}***{local[^1]}";
            return $"{masked}@{parts[1]}";
        }
        return MaskPhone(id);
    }
}
