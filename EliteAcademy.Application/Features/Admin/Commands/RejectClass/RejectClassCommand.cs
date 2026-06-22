using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Commands.RejectClass;

public record RejectClassCommand(int ClassId, string Feedback) : IRequest<Result<bool>>;
