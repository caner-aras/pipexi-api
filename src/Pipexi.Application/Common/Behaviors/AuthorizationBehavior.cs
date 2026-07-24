using MediatR;

namespace Pipexi.Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _ = request;
        _ = cancellationToken;

        // Authorization policy checks will be plugged in per feature.
        return await next();
    }
}
