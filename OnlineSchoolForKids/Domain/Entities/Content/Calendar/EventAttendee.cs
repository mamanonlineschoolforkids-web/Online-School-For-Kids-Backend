using Domain.Enums.Content;

namespace Domain.Entities.Content.Calendar
{
    public class EventAttendee
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime? JoinedAt { get; set; }
        public AttendeeStatus Status { get; set; } = AttendeeStatus.Registered;
    }
}
