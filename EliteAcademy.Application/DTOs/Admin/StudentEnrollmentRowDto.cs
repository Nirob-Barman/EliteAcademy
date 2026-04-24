namespace EliteAcademy.Application.DTOs.Admin
{
    public class StudentEnrollmentRowDto
    {
        public string?   StudentId   { get; set; }
        public string?   StudentName { get; set; }
        public string?   Email       { get; set; }
        public DateTime  EnrolledAt  { get; set; }
    }
}
