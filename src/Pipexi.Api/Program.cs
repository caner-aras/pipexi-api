using Microsoft.EntityFrameworkCore;
using Workforce.Api.DependencyInjection;
using Workforce.Api.Endpoints.V1;
using Workforce.Api.Middleware;
using Workforce.Application.DependencyInjection;
using Workforce.Infrastructure.DependencyInjection;
using Workforce.Persistence.Context;
using Workforce.Persistence.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var runMigrationsOnStartup = builder.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup");

builder.Services
    .AddApi(builder.Configuration)
    .AddApplication()
    .AddPersistence(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (runMigrationsOnStartup)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestContextMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapOrganizationEndpoints();
app.MapOrganizationMemberEndpoints();
app.MapLocationEndpoints();
app.MapTeamEndpoints();
app.MapShiftEndpoints();
app.MapTimeEntryEndpoints();
app.MapTaskEndpoints();
app.MapFormEndpoints();
app.MapAnnouncementEndpoints();
app.MapLeaveRequestEndpoints();
app.MapNotificationEndpoints();
app.MapAuditLogEndpoints();
app.MapRoleEndpoints();
app.MapPermissionEndpoints();
app.MapRolePermissionEndpoints();
app.MapReportEndpoints();


app.Run();
