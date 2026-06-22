using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.AssignRole;

public record AssignRoleCommand(string UserId, string RoleName) : IRequest<Result<bool>>;
