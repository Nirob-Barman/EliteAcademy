using EliteAcademy.Application.DTOs.Coupon;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Coupon.Commands.CreateCoupon;

public record CreateCouponCommand(CouponFormDto Dto) : IRequest<Result<bool>>;
