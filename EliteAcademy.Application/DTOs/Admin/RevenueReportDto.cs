namespace EliteAcademy.Application.DTOs.Admin
{
    public class RevenueReportDto
    {
        public int                       Year               { get; set; }
        public decimal                   TotalRevenue       { get; set; }
        public int                       TotalTransactions  { get; set; }
        public List<MonthlyRevenueDto>   ByMonth            { get; set; } = new();
        public List<ClassRevenueDto>     ByClass            { get; set; } = new();
        public List<InstructorRevenueDto> ByInstructor      { get; set; } = new();
    }

    public class MonthlyRevenueDto
    {
        public int     Month        { get; set; }
        public string  MonthName    { get; set; } = string.Empty;
        public decimal Revenue      { get; set; }
        public int     Transactions { get; set; }
    }

    public class ClassRevenueDto
    {
        public int     ClassId   { get; set; }
        public string? ClassName { get; set; }
        public decimal Revenue   { get; set; }
        public int     Enrolled  { get; set; }
    }

    public class InstructorRevenueDto
    {
        public string? InstructorId   { get; set; }
        public string? InstructorName { get; set; }
        public decimal Revenue        { get; set; }
        public int     Enrolled       { get; set; }
    }
}
