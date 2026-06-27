using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAllClasses;

public record GetAllClassesQuery(int Page = 1, int PageSize = 15) : IRequest<Result<PagedResult<ClassDto>>>;
