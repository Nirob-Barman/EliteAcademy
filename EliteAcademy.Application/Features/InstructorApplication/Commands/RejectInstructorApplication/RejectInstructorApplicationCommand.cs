using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.InstructorApplication.Commands.RejectInstructorApplication;

public record RejectInstructorApplicationCommand(int ApplicationId, string AdminNotes) : IRequest<Result<bool>>;
