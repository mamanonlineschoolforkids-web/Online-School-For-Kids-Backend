using Domain.Entities;
using Domain.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Commands.Profile.Users;

public class GetNotificationPreferencesCommand : IRequest<NotificationPreferences>
{
    public string UserId { get; set; } = string.Empty;
}

public class GetNotificationPreferencesCommandHandler : IRequestHandler<GetNotificationPreferencesCommand, NotificationPreferences>
{
    private readonly IUserRepository _userRepository;

    public GetNotificationPreferencesCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<NotificationPreferences> Handle(GetNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new KeyNotFoundException("user not found");

        return user.NotificationPreferences ?? new NotificationPreferences
        {
            ProgressUpdates = true,
            WeeklyReports = true,
            AchievementAlerts = true,
            PaymentReminders = true
        };
    }
}