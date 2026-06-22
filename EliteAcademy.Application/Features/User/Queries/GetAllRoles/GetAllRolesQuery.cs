using MediatR;

namespace EliteAcademy.Application.Features.User.Queries.GetAllRoles;

public record GetAllRolesQuery : IRequest<List<string>>;
