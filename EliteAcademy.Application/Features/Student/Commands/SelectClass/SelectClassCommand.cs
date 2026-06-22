using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Commands.SelectClass;

public record SelectClassCommand(int ClassId) : IRequest<Result<bool>>;
