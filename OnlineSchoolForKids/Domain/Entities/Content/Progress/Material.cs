namespace Domain.Entities.Content.Progress
{
    public class Material : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string LessonId { get; set; } = string.Empty;
        public string Type { get; set; }
        public long FileSize { get; set; }
        public Lesson? Lesson { get; set; }
    }
}
