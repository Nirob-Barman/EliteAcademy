using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Commands.BanStudent;

public record BanStudentCommand(string StudentId) : IRequest<Result<bool>>;
