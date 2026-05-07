
using Domain.Entities.Users;
using Domain.Enums.Users;
using Domain.Interfaces.Repositories.Users;
using Domain.Interfaces.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Domain.Interfaces.Services.Shared;

namespace Infrastructure.Data.Seeding;

public static class SpecialistSeeder
{
    public static async Task SeedAsync(IHost app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<User>>();

        try
        {
            var userRepo = services.GetRequiredService<IUserRepository>();
            var hasher = services.GetRequiredService<IPasswordHasher>();

            var specialists = GetSeedSpecialists();

            foreach (var (email, data) in specialists)
            {
                var existing = await userRepo.GetByEmailAsync(email, CancellationToken.None);
                if (existing is not null)
                {
                    logger.LogInformation("Specialist {Email} already exists — skipping.", email);
                    continue;
                }

                var user = new User
                {
                    FullName          = data.FullName,
                    Email             = email,
                    EmailVerified     = true,
                    PasswordHash      = hasher.HashPassword("Specialist@123!"),
                    Role              = UserRole.Specialist,
                    Status            = UserStatus.Active,
                    AuthProvider      = AuthProvider.Local,
                    IsFirstLogin      = false,
                    DateOfBirth       = new DateTime(1985, 1, 1),
                    Country           = data.Country,
                    Bio               = data.Bio,
                    ProfessionalTitle = data.ProfessionalTitle,
                    ExpertiseTags     = data.Specializations,
                    YearsOfExperience = data.YearsOfExperience,
                    HourlyRate        = data.HourlyRate,
                    AverageRating     = data.AverageRating,
                    TotalStudents     = data.TotalStudents,
                    ReviewsCount      = data.ReviewsCount,
                    CreatedAt         = DateTime.UtcNow,
                    ActivityLog       = [],
                    Availability      = data.Availability,
                    Certifications    = data.Certifications,
                    WorkExperiences   = data.WorkExperiences,
                };

                await userRepo.CreateAsync(user, CancellationToken.None);
                logger.LogInformation("Specialist seeded → {Email}", email);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed specialists.");
        }
    }

    // ── Seed data ─────────────────────────────────────────────────────────────

    private record SpecialistSeedData(
        string FullName,
        string Country,
        string Bio,
        string ProfessionalTitle,
        List<string> Specializations,
        int YearsOfExperience,
        decimal HourlyRate,
        double AverageRating,
        int TotalStudents,
        int ReviewsCount,
        List<AvailabilitySlot> Availability,
        List<Certification> Certifications,
        List<WorkExperience> WorkExperiences
    );

    private static Dictionary<string, SpecialistSeedData> GetSeedSpecialists() => new()
    {
        ["emily.chen@maman.com"] = new(
            FullName: "Dr. Emily Chen",
            Country: "United States",
            Bio: "PhD in Educational Psychology with 15+ years helping students overcome ADHD, dyslexia, and anxiety-related learning difficulties. My approach blends evidence-based strategies with personalized coaching.",
            ProfessionalTitle: "Educational Psychologist",
            Specializations: ["ADHD", "Dyslexia", "Anxiety"],
            YearsOfExperience: 15,
            HourlyRate: 150,
            AverageRating: 4.9,
            TotalStudents: 120,
            ReviewsCount: 98,
            Availability:
            [
                new() { Day = "Monday",    StartTime = "09:00", EndTime = "17:00" },
                new() { Day = "Wednesday", StartTime = "09:00", EndTime = "17:00" },
                new() { Day = "Friday",    StartTime = "10:00", EndTime = "14:00" },
            ],
            Certifications:
            [
                new() { Name = "Board Certified Educational Therapist", Issuer = "Association of Educational Therapists", Year = 2012 },
                new() { Name = "Certified ADHD Professional",           Issuer = "CHADD",                                Year = 2015 },
            ],
            WorkExperiences:
            [
                new() { Title = "Senior Educational Psychologist", Place = "Boston Children's Hospital", StartDate = "2015-01-01", EndDate = "2023-01-01", IsCurrentRole = false },
                new() { Title = "Private Practice Specialist",     Place = "Self-employed",              StartDate = "2023-01-01", EndDate = "",            IsCurrentRole = true  },
            ]
        ),

        ["james.wilson@maman.com"] = new(
            FullName: "Dr. James Wilson",
            Country: "United States",
            Bio: "Child psychologist with expertise in behavioral learning strategies and gifted education. I help families unlock their child's full potential through tailored programs.",
            ProfessionalTitle: "Child Psychologist",
            Specializations: ["Gifted Education", "Behavioral Strategies", "Learning Disabilities"],
            YearsOfExperience: 12,
            HourlyRate: 130,
            AverageRating: 4.8,
            TotalStudents: 95,
            ReviewsCount: 74,
            Availability:
            [
                new() { Day = "Tuesday",  StartTime = "10:00", EndTime = "16:00" },
                new() { Day = "Thursday", StartTime = "10:00", EndTime = "16:00" },
                new() { Day = "Saturday", StartTime = "09:00", EndTime = "13:00" },
            ],
            Certifications:
            [
                new() { Name = "Licensed Psychologist",     Issuer = "New York State Education Department", Year = 2013 },
                new() { Name = "Gifted Education Specialist", Issuer = "National Association for Gifted Children", Year = 2017 },
            ],
            WorkExperiences:
            [
                new() { Title = "Child Psychologist",    Place = "Columbia University Medical Center", StartDate = "2013-06-01", EndDate = "2021-06-01", IsCurrentRole = false },
                new() { Title = "Independent Consultant", Place = "Self-employed",                    StartDate = "2021-06-01", EndDate = "",            IsCurrentRole = true  },
            ]
        ),

        ["sarah.martinez@maman.com"] = new(
            FullName: "Sarah Martinez",
            Country: "United States",
            Bio: "Licensed speech-language pathologist specializing in language-based learning disabilities and articulation therapy. I work with children and teens to build confident communication skills.",
            ProfessionalTitle: "Speech-Language Pathologist",
            Specializations: ["Speech Therapy", "Language Disorders", "Dyslexia"],
            YearsOfExperience: 10,
            HourlyRate: 120,
            AverageRating: 4.7,
            TotalStudents: 200,
            ReviewsCount: 163,
            Availability:
            [
                new() { Day = "Monday",    StartTime = "08:00", EndTime = "15:00" },
                new() { Day = "Tuesday",   StartTime = "08:00", EndTime = "15:00" },
                new() { Day = "Thursday",  StartTime = "08:00", EndTime = "15:00" },
            ],
            Certifications:
            [
                new() { Name = "Certificate of Clinical Competence in SLP", Issuer = "American Speech-Language-Hearing Association", Year = 2014 },
            ],
            WorkExperiences:
            [
                new() { Title = "School SLP",              Place = "Los Angeles Unified School District", StartDate = "2014-09-01", EndDate = "2020-06-01", IsCurrentRole = false },
                new() { Title = "Private Practice Therapist", Place = "Self-employed",                   StartDate = "2020-06-01", EndDate = "",            IsCurrentRole = true  },
            ]
        ),

        ["michael.okafor@maman.com"] = new(
            FullName: "Dr. Michael Okafor",
            Country: "United States",
            Bio: "Educational consultant focused on study skills, time management, and executive functioning coaching for teens and college students. Practical, results-driven sessions.",
            ProfessionalTitle: "Educational Consultant",
            Specializations: ["Study Skills", "Time Management", "Executive Functioning"],
            YearsOfExperience: 8,
            HourlyRate: 140,
            AverageRating: 4.9,
            TotalStudents: 180,
            ReviewsCount: 145,
            Availability:
            [
                new() { Day = "Wednesday", StartTime = "12:00", EndTime = "20:00" },
                new() { Day = "Friday",    StartTime = "12:00", EndTime = "20:00" },
                new() { Day = "Sunday",    StartTime = "10:00", EndTime = "16:00" },
            ],
            Certifications:
            [
                new() { Name = "Certified Executive Function Coach", Issuer = "Edge Foundation", Year = 2018 },
                new() { Name = "EdD in Curriculum & Instruction",    Issuer = "Northwestern University", Year = 2016 },
            ],
            WorkExperiences:
            [
                new() { Title = "Academic Dean",           Place = "Chicago Academy High School", StartDate = "2016-08-01", EndDate = "2022-06-01", IsCurrentRole = false },
                new() { Title = "Educational Consultant",  Place = "Self-employed",               StartDate = "2022-06-01", EndDate = "",            IsCurrentRole = true  },
            ]
        ),

        ["layla.hassan@maman.com"] = new(
            FullName: "Layla Hassan",
            Country: "Egypt",
            Bio: "Bilingual learning specialist (Arabic/English) with deep expertise in supporting students with dyscalculia and math-related anxiety. I bridge cultural and linguistic gaps in learning support.",
            ProfessionalTitle: "Bilingual Learning Specialist",
            Specializations: ["Dyscalculia", "Math Anxiety", "Bilingual Education"],
            YearsOfExperience: 7,
            HourlyRate: 90,
            AverageRating: 4.8,
            TotalStudents: 85,
            ReviewsCount: 61,
            Availability:
            [
                new() { Day = "Sunday",    StartTime = "10:00", EndTime = "18:00" },
                new() { Day = "Monday",    StartTime = "10:00", EndTime = "18:00" },
                new() { Day = "Wednesday", StartTime = "14:00", EndTime = "20:00" },
            ],
            Certifications:
            [
                new() { Name = "Certified Dyscalculia Practitioner", Issuer = "British Dyslexia Association", Year = 2019 },
            ],
            WorkExperiences:
            [
                new() { Title = "Learning Support Teacher", Place = "Cairo American College",  StartDate = "2017-09-01", EndDate = "2023-06-01", IsCurrentRole = false },
                new() { Title = "Private Specialist",       Place = "Self-employed",           StartDate = "2023-06-01", EndDate = "",            IsCurrentRole = true  },
            ]
        ),
    };
}