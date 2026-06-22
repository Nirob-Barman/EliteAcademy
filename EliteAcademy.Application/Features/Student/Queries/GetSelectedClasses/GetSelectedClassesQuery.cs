using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Queries.GetSelectedClasses;

public record GetSelectedClassesQuery() : IRequest<Result<List<PreEnrollmentDto>>>;
