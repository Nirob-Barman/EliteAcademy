using EliteAcademy.Application.DTOs.Coupon;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Coupon.Queries.GetAllCoupons;

public record GetAllCouponsQuery : IRequest<Result<List<CouponDto>>>;
