using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Commands.UpdateStudentProfile;

public record UpdateStudentProfileCommand(
    StudentProfileDto Dto,
    Stream? ImageStream,
    string? ImageFileName) : IRequest<Result<bool>>;
