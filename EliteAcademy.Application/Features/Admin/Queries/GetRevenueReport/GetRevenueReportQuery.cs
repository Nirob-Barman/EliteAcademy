using EliteAcademy.Application.DTOs.Admin;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Admin.Queries.GetRevenueReport;

public record GetRevenueReportQuery(int Year) : IRequest<Result<RevenueReportDto>>;
