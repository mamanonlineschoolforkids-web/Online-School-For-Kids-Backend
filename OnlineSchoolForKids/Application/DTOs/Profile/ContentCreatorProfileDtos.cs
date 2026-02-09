using Domain.Entities;


namespace Application.DTOs.Profile;


public class ContentCreatorProfileDto : BaseProfileDto
{
    public bool IsVerifiedCreator { get; set; }
    public List<string> Expertise { get; set; }
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageRating { get; set; }
    public SocialLinks? SocialLinks { get; set; }
    public PayoutSettings? PayoutSettings { get; set; }
}


