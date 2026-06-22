using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Queries.GetEnrolledClasses;

public record GetEnrolledClassesQuery() : IRequest<Result<List<EnrollmentDto>>>;
