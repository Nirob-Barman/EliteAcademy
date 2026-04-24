namespace EliteAcademy.Application.DTOs.Wishlist
{
    public class WishlistDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? ClassImage { get; set; }
        public string? InstructorName { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
    }
}
