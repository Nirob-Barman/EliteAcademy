using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.InstructorApplication.Queries.GetPendingInstructorApplications;

public record GetPendingInstructorApplicationsQuery : IRequest<Result<List<InstructorApplicationDto>>>;
