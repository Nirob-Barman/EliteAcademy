using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.InstructorApplication.Queries.GetAllInstructorApplications;

public record GetAllInstructorApplicationsQuery : IRequest<Result<List<InstructorApplicationDto>>>;
