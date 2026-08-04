using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Pipexi.Domain.Entities;

namespace Pipexi.Persistence.Context;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<LocationWorkingHour> LocationWorkingHours => Set<LocationWorkingHour>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamMemberDayOff> TeamMemberDayOffs => Set<TeamMemberDayOff>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftBreak> ShiftBreaks => Set<ShiftBreak>();
    public DbSet<ShiftRequiredFormTemplate> ShiftRequiredFormTemplates => Set<ShiftRequiredFormTemplate>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<TimeEntryBreak> TimeEntryBreaks => Set<TimeEntryBreak>();
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<FormTemplate> FormTemplates => Set<FormTemplate>();
    public DbSet<FormField> FormFields => Set<FormField>();
    public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();
    public DbSet<FormAnswer> FormAnswers => Set<FormAnswer>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<MemberPositionHistory> MemberPositionHistories => Set<MemberPositionHistory>();
    public DbSet<OrganizationMemberProfile> OrganizationMemberProfiles => Set<OrganizationMemberProfile>();
    public DbSet<OrganizationMemberPayment> OrganizationMemberPayments => Set<OrganizationMemberPayment>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var method = typeof(ApplicationDbContext)
                .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(null, [modelBuilder]);
        }

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(x => x.Status != "deleted");
    }
}
