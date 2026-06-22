using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Queries.GetAdminDashboard;

public record GetAdminDashboardQuery : IRequest<Result<AdminDashboardDto>>;
