using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Coupon.Commands.DeleteCoupon;

public record DeleteCouponCommand(int Id) : IRequest<Result<bool>>;
