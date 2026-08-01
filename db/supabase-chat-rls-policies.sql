-- Tight RLS for hybrid org chat tables.
-- Apply manually after migration. Do NOT use blanket using (true).
-- Client Realtime needs SELECT only; writes go through .NET (service role / direct connection).
--
-- IMPORTANT: Never SELECT conversation_members from a policy ON conversation_members
-- (causes "infinite recursion detected in policy" and Realtime silently drops events).

begin;

alter table public.conversations enable row level security;
alter table public.conversation_members enable row level security;
alter table public.conversation_messages enable row level security;

alter table public.conversation_messages replica identity full;
alter table public.conversation_members replica identity full;

-- SECURITY DEFINER helper bypasses RLS while evaluating membership (avoids recursion).
create or replace function public.is_conversation_member(p_conversation_id uuid)
returns boolean
language sql
stable
security definer
set search_path = public
as $$
  select exists (
    select 1
    from public.conversation_members cm
    join public.organization_members om on om.id = cm.organization_member_id
    join public.users u on u.id = om.user_id
    where cm.conversation_id = p_conversation_id
      and cm.status <> 'deleted'
      and om.status <> 'deleted'
      and (
        u.auth_provider_id = auth.uid()::text
        or u.id = auth.uid()
      )
  );
$$;

revoke all on function public.is_conversation_member(uuid) from public;
grant execute on function public.is_conversation_member(uuid) to authenticated;

drop policy if exists conversations_select_member on public.conversations;
create policy conversations_select_member
on public.conversations
for select
to authenticated
using (public.is_conversation_member(id));

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
using (public.is_conversation_member(conversation_id));

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
using (public.is_conversation_member(conversation_id));

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

    if not exists (
        select 1
        from pg_publication_tables
        where pubname = 'supabase_realtime'
          and schemaname = 'public'
          and tablename = 'conversation_members'
    ) then
        alter publication supabase_realtime add table public.conversation_members;
    end if;
exception
    when undefined_object then
        raise notice 'supabase_realtime publication not found; skip Realtime registration';
end
$$;

commit;
