using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Result<bool>>;
