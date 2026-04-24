namespace EliteAcademy.Application.DTOs.Instructor
{
    public class InstructorDashboardDto
    {
        public int TotalClasses { get; set; }
        public int PendingClasses { get; set; }
        public int ApprovedClasses { get; set; }
        public int RejectedClasses { get; set; }
        public int TotalStudents { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<MonthlyRevenueItem> MonthlyRevenue { get; set; } = new();
    }

    public class MonthlyRevenueItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int Enrollments { get; set; }
        public string Label => $"{new DateTime(Year, Month, 1):MMM yyyy}";
    }
}
