using MediatR;

namespace Workforce.Application.Common.Models;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
