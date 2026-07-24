using MediatR;

namespace Pipexi.Application.Common.Models;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
