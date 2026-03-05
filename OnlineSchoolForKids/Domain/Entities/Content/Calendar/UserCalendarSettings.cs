namespace Domain.Entities.Content.Calendar
{
    public class UserCalendarSettings : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Timezone { get; set; } = "UTC";
        public string DefaultView { get; set; } = "Month"; // Month, Week, Day
        public bool EmailNotifications { get; set; } = true;
        public int ReminderMinutes { get; set; } = 30; // Remind 30 min before
        public List<string> HiddenEventTypes { get; set; } = new();
    }
}
