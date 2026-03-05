using Domain.Entities.Content.Calendar;
using Domain.Enums.Content;
using Domain.Interfaces.Repositories.Content;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using static Application.Commands.Calendar.CreateEventHandler;

namespace Application.Commands.Calendar
{
    public class CreateEventCommand : IRequest<EventDto?>
    {
        public string InstructorId { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public CreateEventDto Dto { get; set; } = new();
    }

    public class CreateEventHandler : IRequestHandler<CreateEventCommand, EventDto?>
    {
        private readonly IEventRepository _eventRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly ILogger<CreateEventHandler> _logger;

        public CreateEventHandler(
            IEventRepository eventRepo,
            ICourseRepository courseRepo,
            ILogger<CreateEventHandler> logger)
        {
            _eventRepo = eventRepo;
            _courseRepo = courseRepo;
            _logger = logger;
        }

        public async Task<EventDto?> Handle(CreateEventCommand request, CancellationToken ct)
        {
            try
            {
                var dto = request.Dto;

                string? courseName = null;
                if (!string.IsNullOrEmpty(dto.CourseId))
                {
                    var course = await _courseRepo.GetByIdAsync(dto.CourseId, ct);
                    courseName = course?.Title;
                }

                var eventEntity = new Event
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = dto.Title,
                    Description = dto.Description,
                    Type = Enum.Parse<EventType>(dto.Type),
                    CourseId = dto.CourseId,
                    CourseName = courseName,
                    InstructorId = request.InstructorId,
                    InstructorName = request.InstructorName,
                    StartDateTime = dto.StartDateTime,
                    EndDateTime = dto.StartDateTime.AddMinutes(dto.Duration),
                    Duration = dto.Duration,
                    MeetingUrl = dto.MeetingUrl,
                    MaxAttendees = dto.MaxAttendees,
                    Color = GetColorForEventType(dto.Type),
                    Attendees = new List<EventAttendee>()
                };

                await _eventRepo.CreateAsync(eventEntity, ct);

                _logger.LogInformation("Event created: {EventId} - {Title}", eventEntity.Id, eventEntity.Title);

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
                    AttendeesCount = 0,
                    IsUserRegistered = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                return null;
            }
        }
        public class UpdateEventCommand : IRequest<bool>
        {
            public string EventId { get; set; } = string.Empty;
            public UpdateEventDto Dto { get; set; } = new();
        }

        public class UpdateEventHandler : IRequestHandler<UpdateEventCommand, bool>
        {
            private readonly IEventRepository _eventRepo;
            private readonly ILogger<UpdateEventHandler> _logger;

            public UpdateEventHandler(
                IEventRepository eventRepo,
                ILogger<UpdateEventHandler> logger)
            {
                _eventRepo = eventRepo;
                _logger = logger;
            }

            public async Task<bool> Handle(UpdateEventCommand request, CancellationToken ct)
            {
                try
                {
                    var eventEntity = await _eventRepo.GetByIdAsync(request.EventId, ct);
                    if (eventEntity == null) return false;

                    eventEntity.Title = request.Dto.Title;
                    eventEntity.Description = request.Dto.Description;
                    eventEntity.StartDateTime = request.Dto.StartDateTime;
                    eventEntity.EndDateTime = request.Dto.StartDateTime.AddMinutes(request.Dto.Duration);
                    eventEntity.Duration = request.Dto.Duration;
                    eventEntity.MeetingUrl = request.Dto.MeetingUrl;

                    await _eventRepo.UpdateAsync(eventEntity.Id, eventEntity, ct);

                    _logger.LogInformation("Event updated: {EventId}", eventEntity.Id);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating event");
                    return false;
                }
            }
        }
        public class DeleteEventCommand : IRequest<bool>
        {
            public string EventId { get; set; } = string.Empty;
        }

        public class DeleteEventHandler : IRequestHandler<DeleteEventCommand, bool>
        {
            private readonly IEventRepository _eventRepo;
            private readonly ILogger<DeleteEventHandler> _logger;

            public DeleteEventHandler(
                IEventRepository eventRepo,
                ILogger<DeleteEventHandler> logger)
            {
                _eventRepo = eventRepo;
                _logger = logger;
            }

            public async Task<bool> Handle(DeleteEventCommand request, CancellationToken ct)
            {
                try
                {
                    await _eventRepo.DeleteAsync(request.EventId, ct);
                    _logger.LogInformation("Event deleted: {EventId}", request.EventId);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting event");
                    return false;
                }
            }
        }
        public class JoinEventCommand : IRequest<JoinEventResponse>
        {
            public string UserId { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string EventId { get; set; } = string.Empty;
        }
        public class JoinEventResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string? MeetingUrl { get; set; }
        }


        public class JoinEventHandler : IRequestHandler<JoinEventCommand, JoinEventResponse>
        {
            private readonly IEventRepository _eventRepo;
            private readonly ILogger<JoinEventHandler> _logger;

            public JoinEventHandler(
                IEventRepository eventRepo,
                ILogger<JoinEventHandler> logger)
            {
                _eventRepo = eventRepo;
                _logger = logger;
            }

            public async Task<JoinEventResponse> Handle(JoinEventCommand request, CancellationToken ct)
            {
                try
                {
                    var eventEntity = await _eventRepo.GetByIdAsync(request.EventId, ct);
                    if (eventEntity == null)
                    {
                        return new JoinEventResponse
                        {
                            Success = false,
                            Message = "Event not found"
                        };
                    }

                    // Check if already registered
                    var alreadyRegistered = eventEntity.Attendees.Any(a => a.UserId == request.UserId);
                    if (alreadyRegistered)
                    {
                        return new JoinEventResponse
                        {
                            Success = true,
                            Message = "Already registered",
                            MeetingUrl = eventEntity.MeetingUrl
                        };
                    }

                    // Check if event is full
                    if (eventEntity.MaxAttendees.HasValue &&
                        eventEntity.Attendees.Count >= eventEntity.MaxAttendees.Value)
                    {
                        return new JoinEventResponse
                        {
                            Success = false,
                            Message = "Event is full"
                        };
                    }

                    // Add attendee
                    eventEntity.Attendees.Add(new EventAttendee
                    {
                        UserId = request.UserId,
                        UserName = request.UserName,
                        JoinedAt = DateTime.UtcNow,
                        Status = AttendeeStatus.Registered
                    });

                    await _eventRepo.UpdateAsync(eventEntity.Id, eventEntity, ct);

                    _logger.LogInformation("User {UserId} joined event {EventId}", request.UserId, request.EventId);

                    return new JoinEventResponse
                    {
                        Success = true,
                        Message = "Successfully joined event",
                        MeetingUrl = eventEntity.MeetingUrl
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error joining event");
                    return new JoinEventResponse
                    {
                        Success = false,
                        Message = "An error occurred"
                    };
                }
            }

        }
        public class EventDto
        {
            public string Id { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string Type { get; set; } = string.Empty; // "LiveSession", "Assignment"
            public string? CourseName { get; set; }
            public string? InstructorName { get; set; }
            public DateTime StartDateTime { get; set; }
            public DateTime? EndDateTime { get; set; }
            public int Duration { get; set; } // Minutes
            public string? MeetingUrl { get; set; }
            public string Color { get; set; } = "#22C55E";
            public int AttendeesCount { get; set; }
            public bool IsUserRegistered { get; set; }
        }
        public class CreateEventDto
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string Type { get; set; } = string.Empty;
            public string? CourseId { get; set; }
            public DateTime StartDateTime { get; set; }
            public int Duration { get; set; } // Minutes
            public string? MeetingUrl { get; set; }
            public int? MaxAttendees { get; set; }
        }
        public class UpdateEventDto
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public DateTime StartDateTime { get; set; }
            public int Duration { get; set; }
            public string? MeetingUrl { get; set; }
        }
        public class JoinEventDto
        {
            public string EventId { get; set; } = string.Empty;
        }

        private static string GetColorForEventType(string type)
        {
            return type switch
            {
                "LiveSession" => "#DCFCE7", // Green
                "Assignment" => "#FEE2E2", // Red
                "StudyGroup" => "#FEF3C7", // Yellow
                "Webinar" => "#DBEAFE", // Blue
                _ => "#F3F4F6" // Gray
            };
        }
    }
}
    
