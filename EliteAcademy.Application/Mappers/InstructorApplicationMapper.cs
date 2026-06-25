using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Domain.Entities;

namespace EliteAcademy.Application.Mappers
{
    public static class InstructorApplicationMapper
    {
        public static InstructorApplicationDto ToDto(InstructorApplication entity) => new()
        {
            Id = entity.Id,
            ApplicantId = entity.ApplicantId,
            FullName = entity.FullName,
            Email = entity.Email,
            Bio = entity.Bio,
            Expertise = entity.Expertise,
            Motivation = entity.Motivation,
            Status = entity.Status,
            AdminNotes = entity.AdminNotes,
            ReviewedAt = entity.ReviewedAt,
            CreatedAt = entity.CreatedAt
        };
    }
}
