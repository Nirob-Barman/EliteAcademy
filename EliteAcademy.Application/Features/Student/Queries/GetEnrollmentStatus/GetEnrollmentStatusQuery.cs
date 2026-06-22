using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Queries.GetEnrollmentStatus;

public record GetEnrollmentStatusQuery() : IRequest<Result<(HashSet<int> Selected, HashSet<int> Enrolled)>>;
