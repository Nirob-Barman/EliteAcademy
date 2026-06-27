using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAllUsers;

public record GetAllUsersQuery(int Page = 1, int PageSize = 15) : IRequest<Result<PagedResult<AdminUserDto>>>;
