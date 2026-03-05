using Domain.Enums.Content;

namespace Domain.Entities.Content.Calendar
{
    public class Event : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public EventType Type { get; set; }
        public string? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? InstructorId { get; set; }
        public string? InstructorName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public int Duration { get; set; } // Minutes
        public string? Location { get; set; } // URL for online events
        public string? MeetingUrl { get; set; }
        public int? MaxAttendees { get; set; }
        public bool IsRecurring { get; set; } = false;
        public string? RecurrencePattern { get; set; } // "Daily", "Weekly", "Monthly"
        public string Color { get; set; } = "#22C55E"; // Tailwind green-500
        public List<EventAttendee> Attendees { get; set; } = new();
        public Course? Course { get; set; }
    }
   
}
