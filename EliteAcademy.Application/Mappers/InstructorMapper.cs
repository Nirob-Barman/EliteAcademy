using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Domain.Entities.Account;

namespace EliteAcademy.Application.Mappers
{
    public static class InstructorMapper
    {
        public static InstructorProfileDto ToProfileDto(AppUser user) => new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            ImageUrl = user.ImageUrl
        };
    }
}
