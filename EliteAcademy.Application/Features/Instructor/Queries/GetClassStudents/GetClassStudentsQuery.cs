using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Instructor.Queries.GetClassStudents;

public record GetClassStudentsQuery(int ClassId) : IRequest<Result<List<ClassStudentDto>>>;
