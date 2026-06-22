using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Class.Commands.UpdateClass;

public record UpdateClassCommand(ClassFormDto Dto, Stream? ImageStream, string? ImageFileName) : IRequest<Result<bool>>;
