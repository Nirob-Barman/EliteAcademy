using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Domain.Entities.Instructor;

namespace EliteAcademy.Application.Mappers
{
    public static class ClassMapper
    {
        public static ClassDto ToDto(Class entity, string? instructorName = null)
        {
            return new ClassDto
            {
                Id = entity.Id,
                ClassName = entity.ClassName,
                ClassImage = entity.ClassImage,
                InstructorId = entity.InstructorId,
                InstructorName = instructorName,
                AvailableSeats = entity.AvailableSeats,
                Price = entity.Price,
                Status = entity.Status,
                Feedback = entity.Feedback
            };
        }

    }
}
