using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Application.DTOs.InstructorApplication
{
    public class InstructorApplicationDto
    {
        public int                          Id           { get; set; }
        public string?                      ApplicantId  { get; set; }
        public string?                      FullName     { get; set; }
        public string?                      Email        { get; set; }
        public string?                      Bio          { get; set; }
        public string?                      Expertise    { get; set; }
        public string?                      Motivation   { get; set; }
        public InstructorApplicationStatus  Status       { get; set; }
        public string?                      AdminNotes   { get; set; }
        public DateTime?                    ReviewedAt   { get; set; }
        public DateTime                     CreatedAt    { get; set; }
    }
}
