using FluentValidation;

namespace EliteAcademy.Application.Features.Coupon.Commands.UpdateCoupon;

public class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponCommandValidator()
    {
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.DiscountPercent).InclusiveBetween(1, 100);
        RuleFor(x => x.Dto.ExpiresAt).GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future.");
    }
}
