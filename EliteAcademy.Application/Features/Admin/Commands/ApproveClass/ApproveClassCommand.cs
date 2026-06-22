using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Commands.ApproveClass;

public record ApproveClassCommand(int ClassId) : IRequest<Result<bool>>;
