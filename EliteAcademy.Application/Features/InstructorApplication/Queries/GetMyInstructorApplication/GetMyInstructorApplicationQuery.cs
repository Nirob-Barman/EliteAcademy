using EliteAcademy.Application.DTOs.InstructorApplication;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.InstructorApplication.Queries.GetMyInstructorApplication;

public record GetMyInstructorApplicationQuery : IRequest<Result<InstructorApplicationDto?>>;
