using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Instructor.Commands.UpdateInstructorProfile;

public record UpdateInstructorProfileCommand(InstructorProfileDto Dto, Stream? ImageStream, string? ImageFileName) : IRequest<Result<bool>>;
