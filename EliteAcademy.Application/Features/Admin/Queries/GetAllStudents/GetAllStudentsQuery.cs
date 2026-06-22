using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAllStudents;

public record GetAllStudentsQuery : IRequest<Result<List<AdminStudentDto>>>;
