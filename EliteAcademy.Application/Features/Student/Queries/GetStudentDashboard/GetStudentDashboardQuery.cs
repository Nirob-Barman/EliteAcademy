using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Queries.GetStudentDashboard;

public record GetStudentDashboardQuery() : IRequest<Result<StudentDashboardDto>>;
