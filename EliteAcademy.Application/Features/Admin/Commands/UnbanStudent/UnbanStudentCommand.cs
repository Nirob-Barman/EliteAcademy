using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Commands.UnbanStudent;

public record UnbanStudentCommand(string StudentId) : IRequest<Result<bool>>;
