using EliteAcademy.Application.Wrappers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EliteAcademy.Application.Behaviors;

public class ExceptionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionBehavior<TRequest, TResponse>> _logger;

    public ExceptionBehavior(ILogger<ExceptionBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[MediatR] Unhandled exception in {Request}",
                typeof(TRequest).Name);

            var responseType = typeof(TResponse);
            if (responseType.IsGenericType &&
                responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var fail = responseType.GetMethod("Fail", new[] { typeof(string) });
                return (TResponse)fail!.Invoke(null, new object[] { "An unexpected error occurred." })!;
            }

            throw;
        }
    }
}
