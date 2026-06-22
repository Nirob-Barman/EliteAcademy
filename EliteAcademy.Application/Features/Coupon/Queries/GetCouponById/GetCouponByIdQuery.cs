using EliteAcademy.Application.DTOs.Coupon;
using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Coupon.Queries.GetCouponById;

public record GetCouponByIdQuery(int Id) : IRequest<Result<CouponDto?>>;
