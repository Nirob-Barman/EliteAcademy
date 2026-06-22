using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.InstructorApplication.Commands.ApplyForInstructor;

public record ApplyForInstructorCommand(InstructorApplicationFormDto Dto) : IRequest<Result<InstructorApplicationDto>>;
