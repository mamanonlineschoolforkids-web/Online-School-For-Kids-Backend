using Domain.Entities;

namespace Application.DTOs.Profile;


public class BaseProfileDto
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Phone { get; set; }
    public string Country { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }

    public NotificationPreferences NotificationPreferences { get; set; }
    public List<PaymentMethodDto> PaymentMethods { get; set; }
}

public class UpdateProfileDto
{
    // Common fields
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? Bio { get; set; }

    // Student-specific
    public string? LearningGoals { get; set; }

    // Parent-specific
    public bool? ParentalControlsActive { get; set; }
    public NotificationPreferences? NotificationPreferences { get; set; }

    // Content Creator-specific
    public List<string>? Expertise { get; set; }
    public SocialLinks? SocialLinks { get; set; }

    // Specialist-specific
    public string? ProfessionalTitle { get; set; }
    public List<string>? Specializations { get; set; }
    public int? YearsOfExperience { get; set; }
    public decimal? HourlyRate { get; set; }
    public SessionRates? SessionRates { get; set; }
    public List<AvailabilitySlotDto>? Availability { get; set; }
}

