namespace Workforce.Application.Abstractions.Observability;

public interface IAppTelemetry
{
    void AddBreadcrumb(string message);
    void CaptureException(Exception exception);
}
