using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Coupon.Commands.ToggleCoupon;

public record ToggleCouponCommand(int Id) : IRequest<Result<bool>>;
