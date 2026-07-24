-- Supabase RLS bootstrap for public tables.
-- This removes the dashboard's unrestricted warning while keeping the API
-- as the main authorization boundary.

begin;

do $$
declare
    table_name text;
    application_tables constant text[] := array[
        'organizations',
        'permissions',
        'users',
        'roles',
        'organization_members',
        'role_permissions',
        'locations',
        'teams',
        'team_members',
        'shifts',
        'shift_breaks',
        'time_entries',
        'time_entry_breaks',
        'tasks',
        'task_comments',
        'files',
        'form_templates',
        'form_fields',
        'form_submissions',
        'form_answers',
        'announcements',
        'audit_logs',
        'leave_requests',
        'notifications',
        'location_working_hours',
        'shift_required_form_templates',
        'team_member_day_offs'
    ];
begin
    foreach table_name in array application_tables
    loop
        execute format(
            'alter table public.%I enable row level security',
            table_name
        );

        execute format(
            'drop policy if exists authenticated_all_access on public.%I',
            table_name
        );

        execute format(
            'create policy authenticated_all_access on public.%I for all to authenticated using (true) with check (true)',
            table_name
        );
    end loop;
end
$$;

alter table public."__EFMigrationsHistory" enable row level security;

drop policy if exists authenticated_no_access on public."__EFMigrationsHistory";

create policy authenticated_no_access
on public."__EFMigrationsHistory"
for all
to authenticated
using (false)
with check (false);

commit;