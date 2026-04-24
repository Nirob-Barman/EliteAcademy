namespace EliteAcademy.Application.DTOs.Admin
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public int PendingClasses { get; set; }
        public int ApprovedClasses { get; set; }
        public int RejectedClasses                { get; set; }
        public int PendingInstructorApplications  { get; set; }
    }
}
