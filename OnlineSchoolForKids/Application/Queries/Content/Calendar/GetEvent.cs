using Domain.Interfaces.Repositories.Content;
using MediatR;
using Microsoft.Extensions.Logging;
using static Application.Commands.Calendar.CreateEventHandler;

namespace Application.Queries.Content.Calendar
{
    public class GetEventByIdQuery : IRequest<EventDto?>
    {
        public string UserId { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
    }

    public class GetEventByIdHandler : IRequestHandler<GetEventByIdQuery, EventDto?>
    {
        private readonly IEventRepository _eventRepo;
        private readonly ILogger<GetEventByIdHandler> _logger;

        public GetEventByIdHandler(
            IEventRepository eventRepo,
            ILogger<GetEventByIdHandler> logger)
        {
            _eventRepo = eventRepo;
            _logger = logger;
        }

        public async Task<EventDto?> Handle(GetEventByIdQuery request, CancellationToken ct)
        {
            try
            {
                var eventEntity = await _eventRepo.GetByIdAsync(request.EventId, ct);
                if (eventEntity == null) return null;

                var isUserRegistered = eventEntity.Attendees.Any(a => a.UserId == request.UserId);

                return new EventDto
                {
                    Id = eventEntity.Id,
                    Title = eventEntity.Title,
                    Description = eventEntity.Description,
                    Type = eventEntity.Type.ToString(),
                    CourseName = eventEntity.CourseName,
                    InstructorName = eventEntity.InstructorName,
                    StartDateTime = eventEntity.StartDateTime,
                    EndDateTime = eventEntity.EndDateTime,
                    Duration = eventEntity.Duration,
                    MeetingUrl = eventEntity.MeetingUrl,
                    Color = eventEntity.Color,
                    AttendeesCount = eventEntity.Attendees.Count,
                    IsUserRegistered = isUserRegistered
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event by ID");
                return null;
            }
        }
    }
}