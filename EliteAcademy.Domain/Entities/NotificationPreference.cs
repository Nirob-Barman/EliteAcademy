namespace EliteAcademy.Domain.Entities
{
    public class NotificationPreference
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Email notifications
        public bool EmailOnEnrollment        { get; set; } = true;
        public bool EmailOnAnnouncement      { get; set; } = true;
        public bool EmailOnClassStatus       { get; set; } = true;
        public bool EmailOnApplicationStatus { get; set; } = true;
        public bool EmailOnPasswordChange    { get; set; } = true;

        // In-app notifications
        public bool InAppOnEnrollment   { get; set; } = true;
        public bool InAppOnAnnouncement { get; set; } = true;

        public void UpdatePreferences(
            bool emailOnEnrollment, bool emailOnAnnouncement, bool emailOnClassStatus,
            bool emailOnApplicationStatus, bool emailOnPasswordChange,
            bool inAppOnEnrollment, bool inAppOnAnnouncement)
        {
            EmailOnEnrollment = emailOnEnrollment;
            EmailOnAnnouncement = emailOnAnnouncement;
            EmailOnClassStatus = emailOnClassStatus;
            EmailOnApplicationStatus = emailOnApplicationStatus;
            EmailOnPasswordChange = emailOnPasswordChange;
            InAppOnEnrollment = inAppOnEnrollment;
            InAppOnAnnouncement = inAppOnAnnouncement;
        }
    }
}
