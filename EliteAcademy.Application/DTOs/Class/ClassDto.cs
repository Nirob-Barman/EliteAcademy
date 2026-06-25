using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Application.DTOs.Class
{
    public class ClassDto
    {
        public int Id { get; set; }
        public string? ClassName { get; set; }
        public string? ClassImage { get; set; }
        public string? InstructorId { get; set; }
        public string? InstructorName { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Price { get; set; }
        public ClassStatus Status { get; set; }
        public string? Feedback { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
