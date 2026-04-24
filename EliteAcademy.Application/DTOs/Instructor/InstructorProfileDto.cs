namespace EliteAcademy.Application.DTOs.Instructor
{
    public class InstructorProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? ImageUrl { get; set; }
        public int ClassCount { get; set; }
        public int StudentCount { get; set; }
    }
}
