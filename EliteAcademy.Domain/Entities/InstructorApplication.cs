using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Domain.Entities
{
    public class InstructorApplication : BaseEntity
    {
        public string?                      ApplicantId  { get; set; }   // FK → Identity user
        public string?                      FullName     { get; set; }
        public string?                      Email        { get; set; }
        public string?                      Bio          { get; set; }
        public string?                      Expertise    { get; set; }
        public string?                      Motivation   { get; set; }
        public InstructorApplicationStatus  Status       { get; set; } = InstructorApplicationStatus.Pending;
        public string?                      AdminNotes   { get; set; }
        public DateTime?                    ReviewedAt   { get; set; }
    }
}
