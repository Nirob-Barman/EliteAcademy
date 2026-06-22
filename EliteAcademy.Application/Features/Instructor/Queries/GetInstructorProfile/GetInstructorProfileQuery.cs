using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Instructor.Queries.GetInstructorProfile;

public record GetInstructorProfileQuery : IRequest<Result<InstructorProfileDto>>;
