using EliteAcademy.Application.DTOs.Wishlist;
using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Domain.Entities.Student;

namespace EliteAcademy.Application.Mappers
{
    public static class WishlistMapper
    {
        public static WishlistDto ToDto(Wishlist entity, Class? cls, string? instructorName) => new()
        {
            Id = entity.Id,
            ClassId = entity.ClassId,
            ClassName = cls?.ClassName,
            ClassImage = cls?.ClassImage,
            InstructorName = instructorName,
            Price = cls?.Price ?? 0,
            AvailableSeats = cls?.AvailableSeats ?? 0
        };
    }
}
