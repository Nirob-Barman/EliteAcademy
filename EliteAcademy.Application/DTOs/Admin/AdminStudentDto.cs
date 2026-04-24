namespace EliteAcademy.Application.DTOs.Admin
{
    public class AdminStudentDto
    {
        public string?  Id              { get; set; }
        public string?  FullName        { get; set; }
        public string?  Email           { get; set; }
        public int      EnrollmentCount { get; set; }
        public bool     IsBanned        { get; set; }
        public DateTime JoinedAt        { get; set; }
    }
}
