using MediatR;
using Microsoft.Extensions.Logging;

namespace EliteAcademy.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        _logger.LogInformation("[MediatR] Handling {Request}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("[MediatR] Handled  {Request}", typeof(TRequest).Name);
        return response;
    }
}
