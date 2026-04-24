namespace EliteAcademy.Application.DTOs.Admin
{
    public class AdminClassEnrollmentsDto
    {
        public int      ClassId        { get; set; }
        public string?  ClassName      { get; set; }
        public string?  InstructorName { get; set; }
        public decimal  Price          { get; set; }
        public int      AvailableSeats { get; set; }
        public List<StudentEnrollmentRowDto> Enrollments { get; set; } = new();
    }
}
