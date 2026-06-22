using EliteAcademy.Application.DTOs.Home;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Queries.GetPlatformStats;

public record GetPlatformStatsQuery : IRequest<Result<PlatformStatsDto>>;
