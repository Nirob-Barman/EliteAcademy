using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAllStudents;

public record GetAllStudentsQuery(int Page = 1, int PageSize = 15) : IRequest<Result<PagedResult<AdminStudentDto>>>;
