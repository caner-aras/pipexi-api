using MediatR;

namespace Workforce.Application.Common.Models;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
