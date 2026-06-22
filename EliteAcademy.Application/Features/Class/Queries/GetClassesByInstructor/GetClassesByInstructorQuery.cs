using EliteAcademy.Application.DTOs.Class;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Class.Queries.GetClassesByInstructor;

public record GetClassesByInstructorQuery : IRequest<Result<List<ClassDto>>>;
