using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.InstructorApplication.Commands.ApproveInstructorApplication;

public record ApproveInstructorApplicationCommand(int ApplicationId) : IRequest<Result<bool>>;
