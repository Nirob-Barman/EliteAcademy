using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.Logout;

public record LogoutCommand : IRequest<Result<string>>;
