using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Queries.GeneratePasswordResetToken;

public record GeneratePasswordResetTokenQuery(string Email) : IRequest<Result<string>>;
