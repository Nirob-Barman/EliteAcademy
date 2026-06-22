using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Instructor.Queries.GetInstructorDashboard;

public record GetInstructorDashboardQuery : IRequest<Result<InstructorDashboardDto>>;
