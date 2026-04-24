namespace EliteAcademy.Application.DTOs.Review
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
