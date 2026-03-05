namespace Domain.Enums.Content
{
    public enum EventType
    {
        LiveSession = 1,      // Q&A sessions
        Assignment = 2,        // Assignment deadlines
        StudyGroup = 3,       // Study groups
        Webinar = 4,          // Webinars
        Exam = 5,             // Exams
        Deadline = 6,         // General deadlines
        Other = 7
    }
    public enum AttendeeStatus
    {
        Registered = 1,
        Attended = 2,
        Absent = 3,
        Cancelled = 4
    }

}
