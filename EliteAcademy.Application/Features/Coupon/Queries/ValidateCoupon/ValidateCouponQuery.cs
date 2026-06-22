using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Coupon.Queries.ValidateCoupon;

public record ValidateCouponQuery(string Code, decimal Price) : IRequest<Result<decimal>>;
