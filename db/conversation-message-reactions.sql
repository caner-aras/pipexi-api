-- Chat message reactions column (apply if migrations are not auto-run)
alter table public.conversation_messages
  add column if not exists reactions_json jsonb null;
