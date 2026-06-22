using EliteAcademy.Application.DTOs.Account;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.User.Queries.GetMyLoginHistory;

public record GetMyLoginHistoryQuery : IRequest<Result<List<LoginHistoryItemDto>>>;
