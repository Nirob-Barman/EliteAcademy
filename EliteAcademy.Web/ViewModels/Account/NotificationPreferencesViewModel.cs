namespace EliteAcademy.Web.ViewModels.Account
{
    public class NotificationPreferencesViewModel
    {
        public bool EmailOnEnrollment        { get; set; }
        public bool EmailOnAnnouncement      { get; set; }
        public bool EmailOnClassStatus       { get; set; }
        public bool EmailOnApplicationStatus { get; set; }
        public bool EmailOnPasswordChange    { get; set; }
        public bool InAppOnEnrollment        { get; set; }
        public bool InAppOnAnnouncement      { get; set; }
    }
}
