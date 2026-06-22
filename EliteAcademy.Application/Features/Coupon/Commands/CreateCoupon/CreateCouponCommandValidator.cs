using FluentValidation;

namespace EliteAcademy.Application.Features.Coupon.Commands.CreateCoupon;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.DiscountPercent).InclusiveBetween(1, 100);
        RuleFor(x => x.Dto.ExpiresAt).GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future.");
    }
}
