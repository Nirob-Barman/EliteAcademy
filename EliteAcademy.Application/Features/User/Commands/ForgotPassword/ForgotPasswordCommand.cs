using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email, string CallbackUrl) : IRequest<Result<bool>>;
