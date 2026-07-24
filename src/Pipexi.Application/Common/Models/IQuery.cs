using MediatR;

namespace Pipexi.Application.Common.Models;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
