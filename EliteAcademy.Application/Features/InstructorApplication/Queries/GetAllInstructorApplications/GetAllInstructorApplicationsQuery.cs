using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.InstructorApplication.Queries.GetAllInstructorApplications;

public record GetAllInstructorApplicationsQuery(int Page = 1, int PageSize = 15) : IRequest<Result<PagedResult<InstructorApplicationDto>>>;
