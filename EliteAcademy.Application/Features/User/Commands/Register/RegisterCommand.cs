using EliteAcademy.Application.DTOs.Account;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Commands.Register;

public record RegisterCommand(RegisterDto Model, Stream? ImageStream, string? ImageFileName) : IRequest<Result<string>>;
