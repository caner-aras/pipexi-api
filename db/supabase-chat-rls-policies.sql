-- Tight RLS for hybrid org chat tables.
-- Apply manually after migration. Do NOT use blanket using (true).
-- Client Realtime needs SELECT only; writes go through .NET (service role / direct connection).

begin;

alter table public.conversations enable row level security;
alter table public.conversation_members enable row level security;
alter table public.conversation_messages enable row level security;

-- Helper: current auth user's organization_member ids (active).
-- Join path: auth.uid() -> users.auth_provider_id -> organization_members

drop policy if exists conversations_select_member on public.conversations;
create policy conversations_select_member
on public.conversations
for select
to authenticated
using (
    exists (
        select 1
        from public.conversation_members cm
        join public.organization_members om on om.id = cm.organization_member_id
        join public.users u on u.id = om.user_id
        where cm.conversation_id = conversations.id
          and cm.status <> 'deleted'
          and om.status <> 'deleted'
          and u.auth_provider_id = auth.uid()::text
    )
);

drop policy if exists conversations_no_client_write on public.conversations;
create policy conversations_no_client_write
on public.conversations
for insert
to authenticated
with check (false);

drop policy if exists conversations_no_client_update on public.conversations;
create policy conversations_no_client_update
on public.conversations
for update
to authenticated
using (false)
with check (false);

drop policy if exists conversations_no_client_delete on public.conversations;
create policy conversations_no_client_delete
on public.conversations
for delete
to authenticated
using (false);

drop policy if exists conversation_members_select_member on public.conversation_members;
create policy conversation_members_select_member
on public.conversation_members
for select
to authenticated
using (
    exists (
        select 1
        from public.conversation_members self_cm
        join public.organization_members om on om.id = self_cm.organization_member_id
        join public.users u on u.id = om.user_id
        where self_cm.conversation_id = conversation_members.conversation_id
          and self_cm.status <> 'deleted'
          and om.status <> 'deleted'
          and u.auth_provider_id = auth.uid()::text
    )
);

drop policy if exists conversation_members_no_client_insert on public.conversation_members;
create policy conversation_members_no_client_insert
on public.conversation_members
for insert
to authenticated
with check (false);

drop policy if exists conversation_members_no_client_update on public.conversation_members;
create policy conversation_members_no_client_update
on public.conversation_members
for update
to authenticated
using (false)
with check (false);

drop policy if exists conversation_members_no_client_delete on public.conversation_members;
create policy conversation_members_no_client_delete
on public.conversation_members
for delete
to authenticated
using (false);

drop policy if exists conversation_messages_select_member on public.conversation_messages;
create policy conversation_messages_select_member
on public.conversation_messages
for select
to authenticated
using (
    exists (
        select 1
        from public.conversation_members cm
        join public.organization_members om on om.id = cm.organization_member_id
        join public.users u on u.id = om.user_id
        where cm.conversation_id = conversation_messages.conversation_id
          and cm.status <> 'deleted'
          and om.status <> 'deleted'
          and u.auth_provider_id = auth.uid()::text
    )
);

drop policy if exists conversation_messages_no_client_insert on public.conversation_messages;
create policy conversation_messages_no_client_insert
on public.conversation_messages
for insert
to authenticated
with check (false);

drop policy if exists conversation_messages_no_client_update on public.conversation_messages;
create policy conversation_messages_no_client_update
on public.conversation_messages
for update
to authenticated
using (false)
with check (false);

drop policy if exists conversation_messages_no_client_delete on public.conversation_messages;
create policy conversation_messages_no_client_delete
on public.conversation_messages
for delete
to authenticated
using (false);

-- Realtime: ensure conversation_messages is in the publication (idempotent).
do $$
begin
    if not exists (
        select 1
        from pg_publication_tables
        where pubname = 'supabase_realtime'
          and schemaname = 'public'
          and tablename = 'conversation_messages'
    ) then
        alter publication supabase_realtime add table public.conversation_messages;
    end if;
exception
    when undefined_object then
        raise notice 'supabase_realtime publication not found; skip Realtime registration';
end
$$;

commit;
