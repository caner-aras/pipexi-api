using System.Linq.Expressions;

namespace Pipexi.Application.Abstractions.Jobs;

public interface IBackgroundJobScheduler
{
    string Enqueue<TJob>(Expression<Func<TJob, Task>> jobExpression);
}
