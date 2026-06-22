using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAllClasses;

public record GetAllClassesQuery : IRequest<Result<List<ClassDto>>>;
