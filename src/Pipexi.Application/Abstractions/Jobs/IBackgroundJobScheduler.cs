using System.Linq.Expressions;

namespace Workforce.Application.Abstractions.Jobs;

public interface IBackgroundJobScheduler
{
    string Enqueue<TJob>(Expression<Func<TJob, Task>> jobExpression);
}
