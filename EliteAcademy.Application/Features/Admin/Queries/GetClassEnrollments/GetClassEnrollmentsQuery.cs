using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Queries.GetClassEnrollments;

public record GetClassEnrollmentsQuery(int ClassId) : IRequest<Result<AdminClassEnrollmentsDto>>;
