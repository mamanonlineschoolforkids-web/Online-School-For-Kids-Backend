using Application.DTOs.Profile;
using Domain.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Commands.Profile.Users;

public class GetPublicProfileCommand : IRequest<PublicProfileDto>
{
    public string UserId { get; set; } = string.Empty;
}


public class GetPublicProfileQueryHandler : IRequestHandler<GetPublicProfileCommand, PublicProfileDto>
{
    private readonly IUserRepository _userRepository;
    // Add these repositories if you have them:
    // private readonly ICourseRepository _courseRepository;
    // private readonly IAchievementRepository _achievementRepository;

    public GetPublicProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PublicProfileDto> Handle(GetPublicProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        // Build public profile DTO with only public information
        var publicProfile = new PublicProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            ProfilePictureUrl = user.ProfilePictureUrl,
            Bio = user.Bio,
            Country = user.Country,
            JoinedDate = user.CreatedAt,

            // These would come from your actual data
            // For now, using placeholder values
            EnrolledCourses = 0,
            Achievements = 0,
            TotalHoursLearned = 0,
        };

        // TODO: Fetch real data from your repositories
        // Example:
        // var courses = await _courseRepository.GetUserCoursesAsync(user.Id, cancellationToken);
        // publicProfile.EnrolledCourses = courses.Count();
        // publicProfile.EnrolledCoursesList = courses.Take(5).Select(c => new EnrolledCourseDto
        // {
        //     Name = c.Name,
        //     Instructor = c.InstructorName,
        //     Progress = c.CompletionPercentage
        // }).ToList();

        // var achievements = await _achievementRepository.GetUserAchievementsAsync(user.Id, cancellationToken);
        // publicProfile.Achievements = achievements.Count();
        // publicProfile.RecentAchievements = achievements
        //     .OrderByDescending(a => a.EarnedDate)
        //     .Take(4)
        //     .Select(a => new RecentAchievementDto
        //     {
        //         Name = a.Name,
        //         EarnedDate = a.EarnedDate
        //     }).ToList();

        return publicProfile;
    }
}












