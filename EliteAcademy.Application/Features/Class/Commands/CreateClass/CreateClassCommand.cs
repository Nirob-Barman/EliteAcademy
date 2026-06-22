using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Class.Commands.CreateClass;

public record CreateClassCommand(ClassFormDto Dto, Stream? ImageStream, string? ImageFileName) : IRequest<Result<int>>;
