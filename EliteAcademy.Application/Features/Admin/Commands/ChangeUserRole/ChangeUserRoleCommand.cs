using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Commands.ChangeUserRole;

public record ChangeUserRoleCommand(string UserId, string NewRole) : IRequest<Result<bool>>;
