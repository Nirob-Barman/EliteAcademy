using EliteAcademy.Application.DTOs.Coupon;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Coupon.Commands.UpdateCoupon;

public record UpdateCouponCommand(int Id, CouponFormDto Dto) : IRequest<Result<bool>>;
