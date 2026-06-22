using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Queries.GetAllRolesNonAdmin;

public record GetAllRolesNonAdminQuery : IRequest<Result<List<string>>>;
