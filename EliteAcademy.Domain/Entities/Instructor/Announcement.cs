namespace EliteAcademy.Domain.Entities.Instructor
{
    public class Announcement : BaseEntity
    {
        public int ClassId { get; set; }
        public Class? Class { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
