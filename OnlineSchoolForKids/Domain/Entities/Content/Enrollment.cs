using Domain.Entities.Users;

namespace Domain.Entities.Content
{
    public class Enrollment : BaseEntity
    {
        public int Progress { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public User? User { get; set; }
        public Course? Course
        {
            get; set;
        }
    }
}
    


