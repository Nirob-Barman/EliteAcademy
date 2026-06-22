using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<Result<List<AdminUserDto>>>;
