using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.CreateRole;

public record CreateRoleCommand(string RoleName) : IRequest<Result<bool>>;
