using EliteAcademy.Application.DTOs.Instructor;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Instructor.Queries.GetPublicInstructorList;

public record GetPublicInstructorListQuery : IRequest<Result<List<InstructorProfileDto>>>;
