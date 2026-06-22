using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Class.Queries.GetClassById;

public record GetClassByIdQuery(int Id) : IRequest<Result<ClassDto>>;
