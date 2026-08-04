using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Persistence.Context;
using Pipexi.Persistence.Repositories;
using Pipexi.Persistence.UnitOfWork;

namespace Pipexi.Persistence.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
        }

        services.AddScoped<Pipexi.Persistence.Interceptors.DomainEventDispatcherInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<Pipexi.Persistence.Interceptors.DomainEventDispatcherInterceptor>();
            options.UseNpgsql(connectionString).AddInterceptors(interceptor);
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<ILocationWorkingHourRepository, LocationWorkingHourRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
        services.AddScoped<ITeamMemberDayOffRepository, TeamMemberDayOffRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IShiftBreakRepository, ShiftBreakRepository>();
        services.AddScoped<IShiftRequiredFormTemplateRepository, ShiftRequiredFormTemplateRepository>();
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<ITimeEntryBreakRepository, TimeEntryBreakRepository>();
        services.AddScoped<IWorkTaskRepository, WorkTaskRepository>();
        services.AddScoped<ITaskCommentRepository, TaskCommentRepository>();
        services.AddScoped<IFormTemplateRepository, FormTemplateRepository>();
        services.AddScoped<IFormFieldRepository, FormFieldRepository>();
        services.AddScoped<IFormSubmissionRepository, FormSubmissionRepository>();
        services.AddScoped<IFormAnswerRepository, FormAnswerRepository>();
        services.AddScoped<IStoredFileRepository, StoredFileRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IOrganizationMemberRepository, OrganizationMemberRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IMemberPositionHistoryRepository, MemberPositionHistoryRepository>();
        services.AddScoped<IOrganizationMemberProfileRepository, OrganizationMemberProfileRepository>();
        services.AddScoped<IOrganizationMemberPaymentRepository, OrganizationMemberPaymentRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IConversationMemberRepository, ConversationMemberRepository>();
        services.AddScoped<IConversationMessageRepository, ConversationMessageRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
