using EliteAcademy.Application.Wrappers;
using FluentValidation;
using MediatR;

namespace EliteAcademy.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (!failures.Any()) return await next();

        var responseType = typeof(TResponse);
        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var errors = failures.Select(f => f.ErrorMessage).ToList();
            var failField = responseType.GetMethod("Fail", new[] { typeof(List<string>), typeof(string) });
            if (failField != null)
                return (TResponse)failField.Invoke(null, new object[] { errors, "Validation failed." })!;
        }

        throw new ValidationException(failures);
    }
}
