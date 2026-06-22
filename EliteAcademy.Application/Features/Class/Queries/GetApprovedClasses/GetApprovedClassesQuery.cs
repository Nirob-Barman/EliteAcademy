using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Class.Queries.GetApprovedClasses;

public record GetApprovedClassesQuery : IRequest<Result<List<ClassDto>>>;
