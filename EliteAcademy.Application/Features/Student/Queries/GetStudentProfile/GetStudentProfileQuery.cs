using EliteAcademy.Application.DTOs.Student;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Queries.GetStudentProfile;

public record GetStudentProfileQuery() : IRequest<Result<StudentProfileDto>>;
