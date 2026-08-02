-- WARNING: This schema is for context only and is not meant to be run.
-- Table order and constraints may not be valid for execution.

CREATE TABLE public.__EFMigrationsHistory (
  MigrationId character varying NOT NULL,
  ProductVersion character varying NOT NULL,
  CONSTRAINT __EFMigrationsHistory_pkey PRIMARY KEY (MigrationId)
);
CREATE TABLE public.organizations (
  id uuid NOT NULL,
  name character varying NOT NULL,
  slug character varying NOT NULL,
  timezone character varying NOT NULL DEFAULT 'UTC'::character varying,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  currency character varying NOT NULL DEFAULT 'USD'::character varying,
  CONSTRAINT organizations_pkey PRIMARY KEY (id)
);
CREATE TABLE public.permissions (
  id uuid NOT NULL,
  key character varying NOT NULL,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT permissions_pkey PRIMARY KEY (id)
);
CREATE TABLE public.users (
  id uuid NOT NULL,
  auth_provider_id character varying NOT NULL,
  email character varying NOT NULL,
  first_name character varying NOT NULL,
  last_name character varying NOT NULL,
  phone character varying,
  avatar_url character varying,
  CONSTRAINT users_pkey PRIMARY KEY (id)
);
CREATE TABLE public.roles (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  name character varying NOT NULL,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT roles_pkey PRIMARY KEY (id),
  CONSTRAINT FK_roles_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id)
);
CREATE TABLE public.organization_members (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  user_id uuid NOT NULL,
  role_id uuid NOT NULL,
  job_title character varying,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT organization_members_pkey PRIMARY KEY (id),
  CONSTRAINT FK_organization_members_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id),
  CONSTRAINT FK_organization_members_roles_role_id FOREIGN KEY (role_id) REFERENCES public.roles(id),
  CONSTRAINT FK_organization_members_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id)
);
CREATE TABLE public.role_permissions (
  id uuid NOT NULL,
  role_id uuid NOT NULL,
  permission_id uuid NOT NULL,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT role_permissions_pkey PRIMARY KEY (id),
  CONSTRAINT FK_role_permissions_permissions_permission_id FOREIGN KEY (permission_id) REFERENCES public.permissions(id),
  CONSTRAINT FK_role_permissions_roles_role_id FOREIGN KEY (role_id) REFERENCES public.roles(id)
);
CREATE TABLE public.locations (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  name character varying NOT NULL,
  address character varying,
  latitude numeric,
  longitude numeric,
  geofence_radius_meters integer NOT NULL DEFAULT 100,
  timezone character varying,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT locations_pkey PRIMARY KEY (id),
  CONSTRAINT FK_locations_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id)
);
CREATE TABLE public.teams (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  name character varying NOT NULL,
  manager_member_id uuid,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  location_id uuid,
  CONSTRAINT teams_pkey PRIMARY KEY (id),
  CONSTRAINT FK_teams_organization_members_manager_member_id FOREIGN KEY (manager_member_id) REFERENCES public.organization_members(id),
  CONSTRAINT FK_teams_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id),
  CONSTRAINT FK_teams_locations_location_id FOREIGN KEY (location_id) REFERENCES public.locations(id)
);
CREATE TABLE public.team_members (
  id uuid NOT NULL,
  team_id uuid NOT NULL,
  organization_member_id uuid NOT NULL,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT team_members_pkey PRIMARY KEY (id),
  CONSTRAINT FK_team_members_organization_members_organization_member_id FOREIGN KEY (organization_member_id) REFERENCES public.organization_members(id),
  CONSTRAINT FK_team_members_teams_team_id FOREIGN KEY (team_id) REFERENCES public.teams(id)
);
CREATE TABLE public.shifts (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  team_id uuid,
  organization_member_id uuid,
  location_id uuid NOT NULL,
  title character varying,
  start_at timestamp with time zone NOT NULL,
  end_at timestamp with time zone NOT NULL,
  notes character varying,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT shifts_pkey PRIMARY KEY (id),
  CONSTRAINT FK_shifts_locations_location_id FOREIGN KEY (location_id) REFERENCES public.locations(id),
  CONSTRAINT FK_shifts_organization_members_organization_member_id FOREIGN KEY (organization_member_id) REFERENCES public.organization_members(id),
  CONSTRAINT FK_shifts_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id),
  CONSTRAINT FK_shifts_teams_team_id FOREIGN KEY (team_id) REFERENCES public.teams(id)
);
CREATE TABLE public.shift_breaks (
  id uuid NOT NULL,
  shift_id uuid NOT NULL,
  start_at timestamp with time zone NOT NULL,
  end_at timestamp with time zone NOT NULL,
  is_paid boolean NOT NULL DEFAULT true,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT shift_breaks_pkey PRIMARY KEY (id),
  CONSTRAINT FK_shift_breaks_shifts_shift_id FOREIGN KEY (shift_id) REFERENCES public.shifts(id)
);
CREATE TABLE public.time_entries (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  shift_id uuid NOT NULL,
  organization_member_id uuid NOT NULL,
  location_id uuid NOT NULL,
  clock_in_at timestamp with time zone NOT NULL,
  clock_out_at timestamp with time zone,
  employee_note character varying,
  manager_note character varying,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT time_entries_pkey PRIMARY KEY (id),
  CONSTRAINT FK_time_entries_locations_location_id FOREIGN KEY (location_id) REFERENCES public.locations(id),
  CONSTRAINT FK_time_entries_organization_members_organization_member_id FOREIGN KEY (organization_member_id) REFERENCES public.organization_members(id),
  CONSTRAINT FK_time_entries_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id),
  CONSTRAINT FK_time_entries_shifts_shift_id FOREIGN KEY (shift_id) REFERENCES public.shifts(id)
);
CREATE TABLE public.time_entry_breaks (
  id uuid NOT NULL,
  time_entry_id uuid NOT NULL,
  start_at timestamp with time zone NOT NULL,
  end_at timestamp with time zone NOT NULL,
  is_paid boolean NOT NULL DEFAULT true,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT time_entry_breaks_pkey PRIMARY KEY (id),
  CONSTRAINT FK_time_entry_breaks_time_entries_time_entry_id FOREIGN KEY (time_entry_id) REFERENCES public.time_entries(id)
);
CREATE TABLE public.tasks (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  shift_id uuid,
  location_id uuid,
  title character varying NOT NULL,
  description character varying,
  assigned_to_team_member_id uuid,
  assigned_to_team_id uuid,
  due_at timestamp with time zone,
  priority character varying NOT NULL DEFAULT 'medium'::character varying,
  status character varying NOT NULL DEFAULT 'open'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  reporter_user_id uuid,
  CONSTRAINT tasks_pkey PRIMARY KEY (id),
  CONSTRAINT FK_tasks_locations_location_id FOREIGN KEY (location_id) REFERENCES public.locations(id),
  CONSTRAINT FK_tasks_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id),
  CONSTRAINT FK_tasks_shifts_shift_id FOREIGN KEY (shift_id) REFERENCES public.shifts(id),
  CONSTRAINT FK_tasks_teams_assigned_to_team_id FOREIGN KEY (assigned_to_team_id) REFERENCES public.teams(id),
  CONSTRAINT FK_tasks_team_members_assigned_to_team_member_id FOREIGN KEY (assigned_to_team_member_id) REFERENCES public.team_members(id),
  CONSTRAINT FK_tasks_users_reporter_user_id FOREIGN KEY (reporter_user_id) REFERENCES public.users(id)
);
CREATE TABLE public.task_comments (
  id uuid NOT NULL,
  task_id uuid NOT NULL,
  team_member_id uuid NOT NULL,
  message character varying NOT NULL,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT task_comments_pkey PRIMARY KEY (id),
  CONSTRAINT FK_task_comments_tasks_task_id FOREIGN KEY (task_id) REFERENCES public.tasks(id),
  CONSTRAINT FK_task_comments_team_members_team_member_id FOREIGN KEY (team_member_id) REFERENCES public.team_members(id)
);
CREATE TABLE public.files (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  file_name character varying NOT NULL,
  content_type character varying NOT NULL,
  storage_path character varying NOT NULL,
  size_bytes bigint NOT NULL,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT files_pkey PRIMARY KEY (id),
  CONSTRAINT FK_files_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id)
);
CREATE TABLE public.form_templates (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  name character varying NOT NULL,
  description character varying,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT form_templates_pkey PRIMARY KEY (id),
  CONSTRAINT FK_form_templates_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id)
);
CREATE TABLE public.form_fields (
  id uuid NOT NULL,
  form_template_id uuid NOT NULL,
  type character varying NOT NULL,
  label character varying NOT NULL,
  is_required boolean NOT NULL DEFAULT false,
  sort_order integer NOT NULL,
  options_json jsonb,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT form_fields_pkey PRIMARY KEY (id),
  CONSTRAINT FK_form_fields_form_templates_form_template_id FOREIGN KEY (form_template_id) REFERENCES public.form_templates(id)
);
CREATE TABLE public.form_submissions (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  form_template_id uuid NOT NULL,
  submitted_by_member_id uuid NOT NULL,
  task_id uuid,
  shift_id uuid,
  submitted_at timestamp with time zone NOT NULL,
  status character varying NOT NULL DEFAULT 'submitted'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT form_submissions_pkey PRIMARY KEY (id),
  CONSTRAINT FK_form_submissions_form_templates_form_template_id FOREIGN KEY (form_template_id) REFERENCES public.form_templates(id),
  CONSTRAINT FK_form_submissions_organization_members_submitted_by_member_id FOREIGN KEY (submitted_by_member_id) REFERENCES public.organization_members(id),
  CONSTRAINT FK_form_submissions_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id),
  CONSTRAINT FK_form_submissions_shifts_shift_id FOREIGN KEY (shift_id) REFERENCES public.shifts(id),
  CONSTRAINT FK_form_submissions_tasks_task_id FOREIGN KEY (task_id) REFERENCES public.tasks(id)
);
CREATE TABLE public.form_answers (
  id uuid NOT NULL,
  form_submission_id uuid NOT NULL,
  form_field_id uuid NOT NULL,
  value text,
  file_id uuid,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT form_answers_pkey PRIMARY KEY (id),
  CONSTRAINT FK_form_answers_files_file_id FOREIGN KEY (file_id) REFERENCES public.files(id),
  CONSTRAINT FK_form_answers_form_fields_form_field_id FOREIGN KEY (form_field_id) REFERENCES public.form_fields(id),
  CONSTRAINT FK_form_answers_form_submissions_form_submission_id FOREIGN KEY (form_submission_id) REFERENCES public.form_submissions(id)
);
CREATE TABLE public.announcements (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  title character varying NOT NULL,
  body character varying NOT NULL,
  audience_type character varying NOT NULL,
  audience_id uuid,
  published_at timestamp with time zone,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT announcements_pkey PRIMARY KEY (id),
  CONSTRAINT FK_announcements_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id)
);
CREATE TABLE public.audit_logs (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  actor_member_id uuid,
  entity_name character varying NOT NULL,
  entity_id uuid NOT NULL,
  action character varying NOT NULL,
  before_json text,
  after_json text,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT audit_logs_pkey PRIMARY KEY (id),
  CONSTRAINT FK_audit_logs_organization_members_actor_member_id FOREIGN KEY (actor_member_id) REFERENCES public.organization_members(id),
  CONSTRAINT FK_audit_logs_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id)
);
CREATE TABLE public.leave_requests (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  organization_member_id uuid NOT NULL,
  leave_type character varying NOT NULL,
  start_date date NOT NULL,
  end_date date NOT NULL,
  reason character varying NOT NULL,
  status character varying NOT NULL DEFAULT 'pending'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT leave_requests_pkey PRIMARY KEY (id),
  CONSTRAINT FK_leave_requests_organization_members_organization_member_id FOREIGN KEY (organization_member_id) REFERENCES public.organization_members(id),
  CONSTRAINT FK_leave_requests_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id)
);
CREATE TABLE public.notifications (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  organization_member_id uuid NOT NULL,
  type character varying NOT NULL,
  title character varying NOT NULL,
  body character varying NOT NULL,
  is_read boolean NOT NULL DEFAULT false,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  scheduled_time timestamp with time zone,
  CONSTRAINT notifications_pkey PRIMARY KEY (id),
  CONSTRAINT FK_notifications_organization_members_organization_member_id FOREIGN KEY (organization_member_id) REFERENCES public.organization_members(id),
  CONSTRAINT FK_notifications_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id)
);
CREATE TABLE public.location_working_hours (
  id uuid NOT NULL,
  location_id uuid NOT NULL,
  day_of_week integer NOT NULL,
  is_closed boolean NOT NULL DEFAULT false,
  opens_at time without time zone,
  closes_at time without time zone,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT location_working_hours_pkey PRIMARY KEY (id),
  CONSTRAINT FK_location_working_hours_locations_location_id FOREIGN KEY (location_id) REFERENCES public.locations(id)
);
CREATE TABLE public.shift_required_form_templates (
  id uuid NOT NULL,
  shift_id uuid NOT NULL,
  form_template_id uuid NOT NULL,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT shift_required_form_templates_pkey PRIMARY KEY (id),
  CONSTRAINT FK_shift_required_form_templates_form_templates_form_template_~ FOREIGN KEY (form_template_id) REFERENCES public.form_templates(id),
  CONSTRAINT FK_shift_required_form_templates_shifts_shift_id FOREIGN KEY (shift_id) REFERENCES public.shifts(id)
);
CREATE TABLE public.team_member_day_offs (
  id uuid NOT NULL,
  team_member_id uuid NOT NULL,
  start_at timestamp with time zone NOT NULL,
  end_at timestamp with time zone NOT NULL,
  reason character varying,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT team_member_day_offs_pkey PRIMARY KEY (id),
  CONSTRAINT FK_team_member_day_offs_team_members_team_member_id FOREIGN KEY (team_member_id) REFERENCES public.team_members(id)
);
CREATE TABLE public.positions (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  title character varying NOT NULL,
  description character varying,
  default_hourly_rate numeric NOT NULL,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT positions_pkey PRIMARY KEY (id),
  CONSTRAINT FK_positions_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id)
);
CREATE TABLE public.member_position_histories (
  id uuid NOT NULL,
  organization_member_id uuid NOT NULL,
  position_id uuid NOT NULL,
  hourly_rate numeric NOT NULL,
  start_date timestamp with time zone NOT NULL,
  end_date timestamp with time zone,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT member_position_histories_pkey PRIMARY KEY (id),
  CONSTRAINT FK_member_position_histories_organization_members_organization~ FOREIGN KEY (organization_member_id) REFERENCES public.organization_members(id),
  CONSTRAINT FK_member_position_histories_positions_position_id FOREIGN KEY (position_id) REFERENCES public.positions(id)
);
CREATE TABLE public.organization_member_payments (
  id uuid NOT NULL,
  organization_member_id uuid NOT NULL,
  amount numeric NOT NULL,
  currency character varying NOT NULL,
  paid_at timestamp with time zone NOT NULL,
  method character varying NOT NULL,
  reference character varying,
  notes character varying,
  period_start date,
  period_end date,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT organization_member_payments_pkey PRIMARY KEY (id),
  CONSTRAINT FK_organization_member_payments_organization_members_organizat~ FOREIGN KEY (organization_member_id) REFERENCES public.organization_members(id)
);
CREATE TABLE public.organization_member_profiles (
  id uuid NOT NULL,
  organization_member_id uuid NOT NULL,
  date_of_birth date,
  gender character varying,
  address_line1 character varying,
  address_line2 character varying,
  city character varying,
  state character varying,
  postal_code character varying,
  country character varying,
  emergency_contact_name character varying,
  emergency_contact_phone character varying,
  national_id character varying,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT organization_member_profiles_pkey PRIMARY KEY (id),
  CONSTRAINT FK_organization_member_profiles_organization_members_organizat~ FOREIGN KEY (organization_member_id) REFERENCES public.organization_members(id)
);
CREATE TABLE public.conversations (
  id uuid NOT NULL,
  organization_id uuid NOT NULL,
  type character varying NOT NULL,
  title character varying,
  direct_member_pair_key character varying,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  CONSTRAINT conversations_pkey PRIMARY KEY (id),
  CONSTRAINT FK_conversations_organizations_organization_id FOREIGN KEY (organization_id) REFERENCES public.organizations(id)
);
CREATE TABLE public.conversation_members (
  id uuid NOT NULL,
  conversation_id uuid NOT NULL,
  organization_member_id uuid NOT NULL,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  last_read_at timestamp with time zone,
  CONSTRAINT conversation_members_pkey PRIMARY KEY (id),
  CONSTRAINT FK_conversation_members_conversations_conversation_id FOREIGN KEY (conversation_id) REFERENCES public.conversations(id),
  CONSTRAINT FK_conversation_members_organization_members_organization_memb~ FOREIGN KEY (organization_member_id) REFERENCES public.organization_members(id)
);
CREATE TABLE public.conversation_messages (
  id uuid NOT NULL,
  conversation_id uuid NOT NULL,
  sender_organization_member_id uuid NOT NULL,
  body character varying NOT NULL,
  status character varying NOT NULL DEFAULT 'active'::character varying,
  created_at timestamp with time zone NOT NULL,
  updated_at timestamp with time zone,
  reactions_json jsonb,
  CONSTRAINT conversation_messages_pkey PRIMARY KEY (id),
  CONSTRAINT FK_conversation_messages_conversations_conversation_id FOREIGN KEY (conversation_id) REFERENCES public.conversations(id),
  CONSTRAINT FK_conversation_messages_organization_members_sender_organizat~ FOREIGN KEY (sender_organization_member_id) REFERENCES public.organization_members(id)
);