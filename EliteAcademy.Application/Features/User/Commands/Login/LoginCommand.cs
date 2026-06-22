using EliteAcademy.Application.DTOs.Account;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.Login;

public record LoginCommand(LoginDto Model) : IRequest<Result<string>>;
