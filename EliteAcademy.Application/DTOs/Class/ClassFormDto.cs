namespace EliteAcademy.Application.DTOs.Class
{
    public class ClassFormDto
    {
        public int Id { get; set; }
        public string? ClassName { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Price { get; set; }
        public string? ExistingImage { get; set; }
    }
}
