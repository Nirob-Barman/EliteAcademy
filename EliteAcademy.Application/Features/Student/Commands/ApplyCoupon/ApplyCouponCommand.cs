using EliteAcademy.Application.Wrappers;
using MediatR;

namespace EliteAcademy.Application.Features.Student.Commands.ApplyCoupon;

public record ApplyCouponCommand(int PreEnrollmentId, string CouponCode) : IRequest<Result<bool>>;
