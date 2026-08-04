CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE TABLE organizations (
        id uuid NOT NULL,
        name character varying(200) NOT NULL,
        slug character varying(200) NOT NULL,
        timezone character varying(100) NOT NULL DEFAULT 'UTC',
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_organizations" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE TABLE permissions (
        id uuid NOT NULL,
        key character varying(100) NOT NULL,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_permissions" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE TABLE users (
        id uuid NOT NULL,
        auth_provider_id character varying(200) NOT NULL,
        email character varying(200) NOT NULL,
        first_name character varying(100) NOT NULL,
        last_name character varying(100) NOT NULL,
        phone character varying(50),
        avatar_url character varying(500),
        CONSTRAINT "PK_users" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE TABLE roles (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        name character varying(100) NOT NULL,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_roles" PRIMARY KEY (id),
        CONSTRAINT "FK_roles_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE TABLE organization_members (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        user_id uuid NOT NULL,
        role_id uuid NOT NULL,
        job_title character varying(100),
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_organization_members" PRIMARY KEY (id),
        CONSTRAINT "FK_organization_members_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE,
        CONSTRAINT "FK_organization_members_roles_role_id" FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_organization_members_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE TABLE role_permissions (
        id uuid NOT NULL,
        role_id uuid NOT NULL,
        permission_id uuid NOT NULL,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_role_permissions" PRIMARY KEY (id),
        CONSTRAINT "FK_role_permissions_permissions_permission_id" FOREIGN KEY (permission_id) REFERENCES permissions (id) ON DELETE CASCADE,
        CONSTRAINT "FK_role_permissions_roles_role_id" FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE UNIQUE INDEX "IX_organization_members_organization_id_user_id" ON organization_members (organization_id, user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE INDEX "IX_organization_members_role_id" ON organization_members (role_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE INDEX "IX_organization_members_user_id" ON organization_members (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE UNIQUE INDEX "IX_organizations_slug" ON organizations (slug);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE UNIQUE INDEX "IX_permissions_key" ON permissions (key);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE INDEX "IX_role_permissions_permission_id" ON role_permissions (permission_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE UNIQUE INDEX "IX_role_permissions_role_id_permission_id" ON role_permissions (role_id, permission_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE UNIQUE INDEX "IX_roles_organization_id_name" ON roles (organization_id, name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE UNIQUE INDEX "IX_users_auth_provider_id" ON users (auth_provider_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    CREATE UNIQUE INDEX "IX_users_email" ON users (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212109_Init') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260709212109_Init', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212819_SeedPermissions') THEN
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('13a2a3f6-f992-4f5f-8e4d-c67c2c0ba809', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'task.read', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('15f8c50d-0ff1-4f79-a1fb-8f11e9624603', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'employee.update', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('4b3330ce-d2bb-442d-bfe6-43dc16ca3c15', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'time.clockin', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('4bff31fb-a5fd-4f7c-a8db-cec95f90d810', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'task.create', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('7b6206f8-08f6-4833-b0c9-95e4a4f50f02', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'employee.create', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('7fc6b247-0809-4249-94ba-ff6260f2de05', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'shift.read', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('806fdd2b-b9a4-4fda-8e28-e72ee0a2f116', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'time.clockout', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('80cc9d06-bba0-4e6c-a02f-b1596f94f408', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'shift.publish', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('89bdd130-7a13-4a99-98cf-d59d7029e607', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'shift.update', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('8fcf3d67-20fb-4225-9e88-6d117f299313', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'report.read', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('968f0c96-c153-4f95-a6ef-6f4558fd0411', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'task.update', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('a5ed558e-09a0-4c2c-91d6-f6ef1c8cab06', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'shift.create', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('c9ce2ea5-f8f1-4f1f-9a74-2b2512f40b12', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'task.complete', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('e9f4dedf-78ea-4bce-afd5-425ff89f5514', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'report.export', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('f0dfb31e-810e-4d83-a08e-10f1b9444104', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'employee.delete', 'active', NULL);
    INSERT INTO permissions (id, created_at, key, status, updated_at)
    VALUES ('f8dfb2dd-70d3-4d59-a0c0-5f4f65ea6e01', TIMESTAMPTZ '2026-07-09T00:00:00+00:00', 'employee.read', 'active', NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260709212819_SeedPermissions') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260709212819_SeedPermissions', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710105650_Location') THEN
    CREATE TABLE locations (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        name character varying(150) NOT NULL,
        address character varying(500),
        latitude numeric(9,6),
        longitude numeric(9,6),
        geofence_radius_meters integer NOT NULL DEFAULT 100,
        timezone character varying(100),
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_locations" PRIMARY KEY (id),
        CONSTRAINT "FK_locations_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710105650_Location') THEN
    CREATE UNIQUE INDEX "IX_locations_organization_id_name" ON locations (organization_id, name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710105650_Location') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260710105650_Location', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710112740_Team_And_Members') THEN
    CREATE TABLE teams (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        name character varying(150) NOT NULL,
        manager_member_id uuid,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_teams" PRIMARY KEY (id),
        CONSTRAINT "FK_teams_organization_members_manager_member_id" FOREIGN KEY (manager_member_id) REFERENCES organization_members (id) ON DELETE SET NULL,
        CONSTRAINT "FK_teams_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710112740_Team_And_Members') THEN
    CREATE TABLE team_members (
        id uuid NOT NULL,
        team_id uuid NOT NULL,
        organization_member_id uuid NOT NULL,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_team_members" PRIMARY KEY (id),
        CONSTRAINT "FK_team_members_organization_members_organization_member_id" FOREIGN KEY (organization_member_id) REFERENCES organization_members (id) ON DELETE CASCADE,
        CONSTRAINT "FK_team_members_teams_team_id" FOREIGN KEY (team_id) REFERENCES teams (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710112740_Team_And_Members') THEN
    CREATE INDEX "IX_team_members_organization_member_id" ON team_members (organization_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710112740_Team_And_Members') THEN
    CREATE UNIQUE INDEX "IX_team_members_team_id_organization_member_id" ON team_members (team_id, organization_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710112740_Team_And_Members') THEN
    CREATE INDEX "IX_teams_manager_member_id" ON teams (manager_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710112740_Team_And_Members') THEN
    CREATE UNIQUE INDEX "IX_teams_organization_id_name" ON teams (organization_id, name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710112740_Team_And_Members') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260710112740_Team_And_Members', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710115029_Shift_And_Break') THEN
    CREATE TABLE shifts (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        team_id uuid,
        organization_member_id uuid,
        location_id uuid NOT NULL,
        title character varying(200),
        start_at timestamp with time zone NOT NULL,
        end_at timestamp with time zone NOT NULL,
        notes character varying(2000),
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_shifts" PRIMARY KEY (id),
        CONSTRAINT "FK_shifts_locations_location_id" FOREIGN KEY (location_id) REFERENCES locations (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_shifts_organization_members_organization_member_id" FOREIGN KEY (organization_member_id) REFERENCES organization_members (id) ON DELETE SET NULL,
        CONSTRAINT "FK_shifts_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE,
        CONSTRAINT "FK_shifts_teams_team_id" FOREIGN KEY (team_id) REFERENCES teams (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710115029_Shift_And_Break') THEN
    CREATE TABLE shift_breaks (
        id uuid NOT NULL,
        shift_id uuid NOT NULL,
        start_at timestamp with time zone NOT NULL,
        end_at timestamp with time zone NOT NULL,
        is_paid boolean NOT NULL DEFAULT TRUE,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_shift_breaks" PRIMARY KEY (id),
        CONSTRAINT "FK_shift_breaks_shifts_shift_id" FOREIGN KEY (shift_id) REFERENCES shifts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710115029_Shift_And_Break') THEN
    CREATE INDEX "IX_shift_breaks_shift_id_start_at" ON shift_breaks (shift_id, start_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710115029_Shift_And_Break') THEN
    CREATE INDEX "IX_shifts_location_id" ON shifts (location_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710115029_Shift_And_Break') THEN
    CREATE INDEX "IX_shifts_organization_id_start_at" ON shifts (organization_id, start_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710115029_Shift_And_Break') THEN
    CREATE INDEX "IX_shifts_organization_member_id" ON shifts (organization_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710115029_Shift_And_Break') THEN
    CREATE INDEX "IX_shifts_team_id" ON shifts (team_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710115029_Shift_And_Break') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260710115029_Shift_And_Break', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710122113_TimeShift_Break') THEN
    CREATE TABLE time_entries (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        shift_id uuid NOT NULL,
        organization_member_id uuid NOT NULL,
        location_id uuid NOT NULL,
        clock_in_at timestamp with time zone NOT NULL,
        clock_out_at timestamp with time zone,
        employee_note character varying(2000),
        manager_note character varying(2000),
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_time_entries" PRIMARY KEY (id),
        CONSTRAINT "FK_time_entries_locations_location_id" FOREIGN KEY (location_id) REFERENCES locations (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_time_entries_organization_members_organization_member_id" FOREIGN KEY (organization_member_id) REFERENCES organization_members (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_time_entries_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE,
        CONSTRAINT "FK_time_entries_shifts_shift_id" FOREIGN KEY (shift_id) REFERENCES shifts (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710122113_TimeShift_Break') THEN
    CREATE TABLE time_entry_breaks (
        id uuid NOT NULL,
        time_entry_id uuid NOT NULL,
        start_at timestamp with time zone NOT NULL,
        end_at timestamp with time zone NOT NULL,
        is_paid boolean NOT NULL DEFAULT TRUE,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_time_entry_breaks" PRIMARY KEY (id),
        CONSTRAINT "FK_time_entry_breaks_time_entries_time_entry_id" FOREIGN KEY (time_entry_id) REFERENCES time_entries (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710122113_TimeShift_Break') THEN
    CREATE INDEX "IX_time_entries_location_id" ON time_entries (location_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710122113_TimeShift_Break') THEN
    CREATE INDEX "IX_time_entries_organization_id_clock_in_at" ON time_entries (organization_id, clock_in_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710122113_TimeShift_Break') THEN
    CREATE INDEX "IX_time_entries_organization_member_id" ON time_entries (organization_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710122113_TimeShift_Break') THEN
    CREATE INDEX "IX_time_entries_shift_id" ON time_entries (shift_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710122113_TimeShift_Break') THEN
    CREATE INDEX "IX_time_entry_breaks_time_entry_id_start_at" ON time_entry_breaks (time_entry_id, start_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710122113_TimeShift_Break') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260710122113_TimeShift_Break', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710123428_Task_TaskComment') THEN
    CREATE TABLE tasks (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        shift_id uuid,
        location_id uuid,
        title character varying(200) NOT NULL,
        description character varying(4000),
        assigned_to_member_id uuid,
        assigned_to_team_id uuid,
        due_at timestamp with time zone,
        priority character varying(20) NOT NULL DEFAULT 'medium',
        status character varying(30) NOT NULL DEFAULT 'open',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_tasks" PRIMARY KEY (id),
        CONSTRAINT "FK_tasks_locations_location_id" FOREIGN KEY (location_id) REFERENCES locations (id) ON DELETE SET NULL,
        CONSTRAINT "FK_tasks_organization_members_assigned_to_member_id" FOREIGN KEY (assigned_to_member_id) REFERENCES organization_members (id) ON DELETE SET NULL,
        CONSTRAINT "FK_tasks_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE,
        CONSTRAINT "FK_tasks_shifts_shift_id" FOREIGN KEY (shift_id) REFERENCES shifts (id) ON DELETE SET NULL,
        CONSTRAINT "FK_tasks_teams_assigned_to_team_id" FOREIGN KEY (assigned_to_team_id) REFERENCES teams (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710123428_Task_TaskComment') THEN
    CREATE TABLE task_comments (
        id uuid NOT NULL,
        task_id uuid NOT NULL,
        organization_member_id uuid NOT NULL,
        message character varying(4000) NOT NULL,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_task_comments" PRIMARY KEY (id),
        CONSTRAINT "FK_task_comments_organization_members_organization_member_id" FOREIGN KEY (organization_member_id) REFERENCES organization_members (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_task_comments_tasks_task_id" FOREIGN KEY (task_id) REFERENCES tasks (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710123428_Task_TaskComment') THEN
    CREATE INDEX "IX_task_comments_organization_member_id" ON task_comments (organization_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710123428_Task_TaskComment') THEN
    CREATE INDEX "IX_task_comments_task_id_created_at" ON task_comments (task_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710123428_Task_TaskComment') THEN
    CREATE INDEX "IX_tasks_assigned_to_member_id" ON tasks (assigned_to_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710123428_Task_TaskComment') THEN
    CREATE INDEX "IX_tasks_assigned_to_team_id" ON tasks (assigned_to_team_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710123428_Task_TaskComment') THEN
    CREATE INDEX "IX_tasks_location_id" ON tasks (location_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710123428_Task_TaskComment') THEN
    CREATE INDEX "IX_tasks_organization_id_due_at" ON tasks (organization_id, due_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710123428_Task_TaskComment') THEN
    CREATE INDEX "IX_tasks_shift_id" ON tasks (shift_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710123428_Task_TaskComment') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260710123428_Task_TaskComment', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125451_Form_Submission_Answer_File') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260710125451_Form_Submission_Answer_File', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE TABLE files (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        file_name character varying(255) NOT NULL,
        content_type character varying(120) NOT NULL,
        storage_path character varying(1000) NOT NULL,
        size_bytes bigint NOT NULL,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_files" PRIMARY KEY (id),
        CONSTRAINT "FK_files_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE TABLE form_templates (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        name character varying(200) NOT NULL,
        description character varying(2000),
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_form_templates" PRIMARY KEY (id),
        CONSTRAINT "FK_form_templates_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE TABLE form_fields (
        id uuid NOT NULL,
        form_template_id uuid NOT NULL,
        type character varying(50) NOT NULL,
        label character varying(250) NOT NULL,
        is_required boolean NOT NULL DEFAULT FALSE,
        sort_order integer NOT NULL,
        options_json jsonb,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_form_fields" PRIMARY KEY (id),
        CONSTRAINT "FK_form_fields_form_templates_form_template_id" FOREIGN KEY (form_template_id) REFERENCES form_templates (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE TABLE form_submissions (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        form_template_id uuid NOT NULL,
        submitted_by_member_id uuid NOT NULL,
        task_id uuid,
        shift_id uuid,
        submitted_at timestamp with time zone NOT NULL,
        status character varying(30) NOT NULL DEFAULT 'submitted',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_form_submissions" PRIMARY KEY (id),
        CONSTRAINT "FK_form_submissions_form_templates_form_template_id" FOREIGN KEY (form_template_id) REFERENCES form_templates (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_form_submissions_organization_members_submitted_by_member_id" FOREIGN KEY (submitted_by_member_id) REFERENCES organization_members (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_form_submissions_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE,
        CONSTRAINT "FK_form_submissions_shifts_shift_id" FOREIGN KEY (shift_id) REFERENCES shifts (id) ON DELETE SET NULL,
        CONSTRAINT "FK_form_submissions_tasks_task_id" FOREIGN KEY (task_id) REFERENCES tasks (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE TABLE form_answers (
        id uuid NOT NULL,
        form_submission_id uuid NOT NULL,
        form_field_id uuid NOT NULL,
        value text,
        file_id uuid,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_form_answers" PRIMARY KEY (id),
        CONSTRAINT "FK_form_answers_files_file_id" FOREIGN KEY (file_id) REFERENCES files (id) ON DELETE SET NULL,
        CONSTRAINT "FK_form_answers_form_fields_form_field_id" FOREIGN KEY (form_field_id) REFERENCES form_fields (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_form_answers_form_submissions_form_submission_id" FOREIGN KEY (form_submission_id) REFERENCES form_submissions (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE INDEX "IX_files_organization_id_created_at" ON files (organization_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE INDEX "IX_form_answers_file_id" ON form_answers (file_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE INDEX "IX_form_answers_form_field_id" ON form_answers (form_field_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE INDEX "IX_form_answers_form_submission_id_form_field_id" ON form_answers (form_submission_id, form_field_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE UNIQUE INDEX "IX_form_fields_form_template_id_sort_order" ON form_fields (form_template_id, sort_order);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE INDEX "IX_form_submissions_form_template_id" ON form_submissions (form_template_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE INDEX "IX_form_submissions_organization_id_submitted_at" ON form_submissions (organization_id, submitted_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE INDEX "IX_form_submissions_shift_id" ON form_submissions (shift_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE INDEX "IX_form_submissions_submitted_by_member_id" ON form_submissions (submitted_by_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE INDEX "IX_form_submissions_task_id" ON form_submissions (task_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    CREATE INDEX "IX_form_templates_organization_id_name" ON form_templates (organization_id, name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710125629_Form_Real_Tables') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260710125629_Form_Real_Tables', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE TABLE announcements (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        title character varying(300) NOT NULL,
        body character varying(8000) NOT NULL,
        audience_type character varying(50) NOT NULL,
        audience_id uuid,
        published_at timestamp with time zone,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_announcements" PRIMARY KEY (id),
        CONSTRAINT "FK_announcements_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE TABLE audit_logs (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        actor_member_id uuid,
        entity_name character varying(120) NOT NULL,
        entity_id uuid NOT NULL,
        action character varying(50) NOT NULL,
        before_json text,
        after_json text,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_audit_logs" PRIMARY KEY (id),
        CONSTRAINT "FK_audit_logs_organization_members_actor_member_id" FOREIGN KEY (actor_member_id) REFERENCES organization_members (id) ON DELETE SET NULL,
        CONSTRAINT "FK_audit_logs_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE TABLE leave_requests (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        organization_member_id uuid NOT NULL,
        leave_type character varying(50) NOT NULL,
        start_date date NOT NULL,
        end_date date NOT NULL,
        reason character varying(2000) NOT NULL,
        status character varying(30) NOT NULL DEFAULT 'pending',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_leave_requests" PRIMARY KEY (id),
        CONSTRAINT "FK_leave_requests_organization_members_organization_member_id" FOREIGN KEY (organization_member_id) REFERENCES organization_members (id) ON DELETE CASCADE,
        CONSTRAINT "FK_leave_requests_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE TABLE notifications (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        organization_member_id uuid NOT NULL,
        type character varying(50) NOT NULL,
        title character varying(300) NOT NULL,
        body character varying(8000) NOT NULL,
        is_read boolean NOT NULL DEFAULT FALSE,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_notifications" PRIMARY KEY (id),
        CONSTRAINT "FK_notifications_organization_members_organization_member_id" FOREIGN KEY (organization_member_id) REFERENCES organization_members (id) ON DELETE CASCADE,
        CONSTRAINT "FK_notifications_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE INDEX "IX_announcements_organization_id_audience_type_audience_id" ON announcements (organization_id, audience_type, audience_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE INDEX "IX_announcements_organization_id_published_at" ON announcements (organization_id, published_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE INDEX "IX_audit_logs_actor_member_id" ON audit_logs (actor_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE INDEX "IX_audit_logs_organization_id_created_at" ON audit_logs (organization_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE INDEX "IX_audit_logs_organization_id_entity_name_entity_id" ON audit_logs (organization_id, entity_name, entity_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE INDEX "IX_leave_requests_organization_id_organization_member_id_start~" ON leave_requests (organization_id, organization_member_id, start_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE INDEX "IX_leave_requests_organization_member_id" ON leave_requests (organization_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE INDEX "IX_notifications_organization_id_organization_member_id_is_read" ON notifications (organization_id, organization_member_id, is_read);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    CREATE INDEX "IX_notifications_organization_member_id" ON notifications (organization_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260710133157_Announcement_LeaveRequest_Notification_AuditLog') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260710133157_Announcement_LeaveRequest_Notification_AuditLog', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260711121222_LocationWorkingHours') THEN
    CREATE TABLE location_working_hours (
        id uuid NOT NULL,
        location_id uuid NOT NULL,
        day_of_week integer NOT NULL,
        is_closed boolean NOT NULL DEFAULT FALSE,
        opens_at time without time zone,
        closes_at time without time zone,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_location_working_hours" PRIMARY KEY (id),
        CONSTRAINT "FK_location_working_hours_locations_location_id" FOREIGN KEY (location_id) REFERENCES locations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260711121222_LocationWorkingHours') THEN
    CREATE UNIQUE INDEX "IX_location_working_hours_location_id_day_of_week" ON location_working_hours (location_id, day_of_week);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260711121222_LocationWorkingHours') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260711121222_LocationWorkingHours', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260711162319_ShiftRequiredForms') THEN
    CREATE TABLE shift_required_form_templates (
        id uuid NOT NULL,
        shift_id uuid NOT NULL,
        form_template_id uuid NOT NULL,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_shift_required_form_templates" PRIMARY KEY (id),
        CONSTRAINT "FK_shift_required_form_templates_form_templates_form_template_~" FOREIGN KEY (form_template_id) REFERENCES form_templates (id) ON DELETE CASCADE,
        CONSTRAINT "FK_shift_required_form_templates_shifts_shift_id" FOREIGN KEY (shift_id) REFERENCES shifts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260711162319_ShiftRequiredForms') THEN
    CREATE INDEX "IX_shift_required_form_templates_form_template_id" ON shift_required_form_templates (form_template_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260711162319_ShiftRequiredForms') THEN
    CREATE UNIQUE INDEX "IX_shift_required_form_templates_shift_id_form_template_id" ON shift_required_form_templates (shift_id, form_template_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260711162319_ShiftRequiredForms') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260711162319_ShiftRequiredForms', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712165927_TeamMemberDayOffsReset') THEN
    CREATE TABLE team_member_day_offs (
        id uuid NOT NULL,
        team_member_id uuid NOT NULL,
        start_at timestamp with time zone NOT NULL,
        end_at timestamp with time zone NOT NULL,
        reason character varying(500),
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_team_member_day_offs" PRIMARY KEY (id),
        CONSTRAINT "FK_team_member_day_offs_team_members_team_member_id" FOREIGN KEY (team_member_id) REFERENCES team_members (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712165927_TeamMemberDayOffsReset') THEN
    CREATE INDEX "IX_team_member_day_offs_team_member_id_start_at_end_at" ON team_member_day_offs (team_member_id, start_at, end_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712165927_TeamMemberDayOffsReset') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260712165927_TeamMemberDayOffsReset', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712181317_TaskAssigneeTeamMember') THEN
    ALTER TABLE tasks DROP CONSTRAINT "FK_tasks_organization_members_assigned_to_member_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712181317_TaskAssigneeTeamMember') THEN
    ALTER TABLE tasks RENAME COLUMN assigned_to_member_id TO assigned_to_team_member_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712181317_TaskAssigneeTeamMember') THEN
    ALTER INDEX "IX_tasks_assigned_to_member_id" RENAME TO "IX_tasks_assigned_to_team_member_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712181317_TaskAssigneeTeamMember') THEN
    UPDATE tasks t
    SET assigned_to_team_member_id = NULL
    WHERE assigned_to_team_member_id IS NOT NULL
      AND NOT EXISTS (
          SELECT 1
          FROM team_members tm
          WHERE tm.id = t.assigned_to_team_member_id
      )
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712181317_TaskAssigneeTeamMember') THEN
    ALTER TABLE tasks ADD CONSTRAINT "FK_tasks_team_members_assigned_to_team_member_id" FOREIGN KEY (assigned_to_team_member_id) REFERENCES team_members (id) ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712181317_TaskAssigneeTeamMember') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260712181317_TaskAssigneeTeamMember', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712203000_TaskCommentAuthorTeamMember') THEN
    ALTER TABLE task_comments DROP CONSTRAINT "FK_task_comments_organization_members_organization_member_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712203000_TaskCommentAuthorTeamMember') THEN
    DROP INDEX "IX_task_comments_organization_member_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712203000_TaskCommentAuthorTeamMember') THEN
    ALTER TABLE task_comments RENAME COLUMN organization_member_id TO team_member_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712203000_TaskCommentAuthorTeamMember') THEN
    WITH mapped AS (
        SELECT DISTINCT ON (tm.organization_member_id)
            tm.organization_member_id,
            tm.id
        FROM team_members tm
        ORDER BY tm.organization_member_id, tm.created_at
    )
    UPDATE task_comments tc
    SET team_member_id = mapped.id
    FROM mapped
    WHERE mapped.organization_member_id = tc.team_member_id;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712203000_TaskCommentAuthorTeamMember') THEN
    DELETE FROM task_comments tc
    WHERE NOT EXISTS (
        SELECT 1
        FROM team_members tm
        WHERE tm.id = tc.team_member_id
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712203000_TaskCommentAuthorTeamMember') THEN
    CREATE INDEX "IX_task_comments_team_member_id" ON task_comments (team_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712203000_TaskCommentAuthorTeamMember') THEN
    ALTER TABLE task_comments ADD CONSTRAINT "FK_task_comments_team_members_team_member_id" FOREIGN KEY (team_member_id) REFERENCES team_members (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712203000_TaskCommentAuthorTeamMember') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260712203000_TaskCommentAuthorTeamMember', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712220000_TaskReporterUser') THEN
    ALTER TABLE tasks ADD reporter_user_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712220000_TaskReporterUser') THEN
    CREATE INDEX "IX_tasks_reporter_user_id" ON tasks (reporter_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712220000_TaskReporterUser') THEN
    ALTER TABLE tasks ADD CONSTRAINT "FK_tasks_users_reporter_user_id" FOREIGN KEY (reporter_user_id) REFERENCES users (id) ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260712220000_TaskReporterUser') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260712220000_TaskReporterUser', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260714122707_Notification_ScheduledTime') THEN
    ALTER TABLE notifications ADD scheduled_time timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260714122707_Notification_ScheduledTime') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260714122707_Notification_ScheduledTime', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260724153857_Positions') THEN
    CREATE TABLE positions (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        title character varying(150) NOT NULL,
        description character varying(500),
        default_hourly_rate numeric(18,2) NOT NULL,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_positions" PRIMARY KEY (id),
        CONSTRAINT "FK_positions_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260724153857_Positions') THEN
    CREATE TABLE member_position_histories (
        id uuid NOT NULL,
        organization_member_id uuid NOT NULL,
        position_id uuid NOT NULL,
        hourly_rate numeric(18,2) NOT NULL,
        start_date timestamp with time zone NOT NULL,
        end_date timestamp with time zone,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_member_position_histories" PRIMARY KEY (id),
        CONSTRAINT "FK_member_position_histories_organization_members_organization~" FOREIGN KEY (organization_member_id) REFERENCES organization_members (id) ON DELETE CASCADE,
        CONSTRAINT "FK_member_position_histories_positions_position_id" FOREIGN KEY (position_id) REFERENCES positions (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260724153857_Positions') THEN
    CREATE INDEX "IX_member_position_histories_organization_member_id_end_date" ON member_position_histories (organization_member_id, end_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260724153857_Positions') THEN
    CREATE INDEX "IX_member_position_histories_position_id" ON member_position_histories (position_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260724153857_Positions') THEN
    CREATE UNIQUE INDEX "IX_positions_organization_id_title" ON positions (organization_id, title);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260724153857_Positions') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260724153857_Positions', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260724162630_Currency') THEN
    ALTER TABLE organizations ADD currency character varying(10) NOT NULL DEFAULT 'USD';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260724162630_Currency') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260724162630_Currency', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730111525_OrganizationMemberProfileAndPayments') THEN
    CREATE TABLE organization_member_payments (
        id uuid NOT NULL,
        organization_member_id uuid NOT NULL,
        amount numeric(18,2) NOT NULL,
        currency character varying(3) NOT NULL,
        paid_at timestamp with time zone NOT NULL,
        method character varying(30) NOT NULL,
        reference character varying(100),
        notes character varying(2000),
        period_start date,
        period_end date,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_organization_member_payments" PRIMARY KEY (id),
        CONSTRAINT "FK_organization_member_payments_organization_members_organizat~" FOREIGN KEY (organization_member_id) REFERENCES organization_members (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730111525_OrganizationMemberProfileAndPayments') THEN
    CREATE TABLE organization_member_profiles (
        id uuid NOT NULL,
        organization_member_id uuid NOT NULL,
        date_of_birth date,
        gender character varying(30),
        address_line1 character varying(200),
        address_line2 character varying(200),
        city character varying(100),
        state character varying(100),
        postal_code character varying(30),
        country character varying(100),
        emergency_contact_name character varying(150),
        emergency_contact_phone character varying(50),
        national_id character varying(50),
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_organization_member_profiles" PRIMARY KEY (id),
        CONSTRAINT "FK_organization_member_profiles_organization_members_organizat~" FOREIGN KEY (organization_member_id) REFERENCES organization_members (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730111525_OrganizationMemberProfileAndPayments') THEN
    CREATE INDEX "IX_organization_member_payments_organization_member_id_paid_at" ON organization_member_payments (organization_member_id, paid_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730111525_OrganizationMemberProfileAndPayments') THEN
    CREATE UNIQUE INDEX "IX_organization_member_profiles_organization_member_id" ON organization_member_profiles (organization_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260730111525_OrganizationMemberProfileAndPayments') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260730111525_OrganizationMemberProfileAndPayments', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731163311_Team_LocationId') THEN
    ALTER TABLE teams ADD location_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731163311_Team_LocationId') THEN
    CREATE INDEX "IX_teams_location_id" ON teams (location_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731163311_Team_LocationId') THEN
    ALTER TABLE teams ADD CONSTRAINT "FK_teams_locations_location_id" FOREIGN KEY (location_id) REFERENCES locations (id) ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731163311_Team_LocationId') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260731163311_Team_LocationId', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731224515_Conversations') THEN
    CREATE TABLE conversations (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        type character varying(30) NOT NULL,
        title character varying(200),
        direct_member_pair_key character varying(80),
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_conversations" PRIMARY KEY (id),
        CONSTRAINT "FK_conversations_organizations_organization_id" FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731224515_Conversations') THEN
    CREATE TABLE conversation_members (
        id uuid NOT NULL,
        conversation_id uuid NOT NULL,
        organization_member_id uuid NOT NULL,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_conversation_members" PRIMARY KEY (id),
        CONSTRAINT "FK_conversation_members_conversations_conversation_id" FOREIGN KEY (conversation_id) REFERENCES conversations (id) ON DELETE CASCADE,
        CONSTRAINT "FK_conversation_members_organization_members_organization_memb~" FOREIGN KEY (organization_member_id) REFERENCES organization_members (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731224515_Conversations') THEN
    CREATE TABLE conversation_messages (
        id uuid NOT NULL,
        conversation_id uuid NOT NULL,
        sender_organization_member_id uuid NOT NULL,
        body character varying(8000) NOT NULL,
        status character varying(30) NOT NULL DEFAULT 'active',
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_conversation_messages" PRIMARY KEY (id),
        CONSTRAINT "FK_conversation_messages_conversations_conversation_id" FOREIGN KEY (conversation_id) REFERENCES conversations (id) ON DELETE CASCADE,
        CONSTRAINT "FK_conversation_messages_organization_members_sender_organizat~" FOREIGN KEY (sender_organization_member_id) REFERENCES organization_members (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731224515_Conversations') THEN
    CREATE UNIQUE INDEX "IX_conversation_members_conversation_id_organization_member_id" ON conversation_members (conversation_id, organization_member_id) WHERE status <> 'deleted';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731224515_Conversations') THEN
    CREATE INDEX "IX_conversation_members_organization_member_id" ON conversation_members (organization_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731224515_Conversations') THEN
    CREATE INDEX "IX_conversation_messages_conversation_id_created_at" ON conversation_messages (conversation_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731224515_Conversations') THEN
    CREATE INDEX "IX_conversation_messages_sender_organization_member_id" ON conversation_messages (sender_organization_member_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731224515_Conversations') THEN
    CREATE INDEX "IX_conversations_organization_id" ON conversations (organization_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731224515_Conversations') THEN
    CREATE UNIQUE INDEX "IX_conversations_organization_id_type_direct_member_pair_key" ON conversations (organization_id, type, direct_member_pair_key) WHERE type = 'direct' AND direct_member_pair_key IS NOT NULL AND status <> 'deleted';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731224515_Conversations') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260731224515_Conversations', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731231009_ConversationMember_LastReadAt') THEN
    ALTER TABLE conversation_members ADD last_read_at timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731231009_ConversationMember_LastReadAt') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260731231009_ConversationMember_LastReadAt', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260801121500_ConversationMessage_ReactionsJson') THEN
    ALTER TABLE conversation_messages ADD reactions_json jsonb;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260801121500_ConversationMessage_ReactionsJson') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260801121500_ConversationMessage_ReactionsJson', '8.0.8');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804210035_AddUserDevices') THEN
    CREATE TABLE user_devices (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        fcm_token character varying(1000) NOT NULL,
        device_type character varying(100),
        status character varying(50) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT "PK_user_devices" PRIMARY KEY (id),
        CONSTRAINT "FK_user_devices_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804210035_AddUserDevices') THEN
    CREATE UNIQUE INDEX "IX_user_devices_fcm_token" ON user_devices (fcm_token);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804210035_AddUserDevices') THEN
    CREATE INDEX "IX_user_devices_user_id" ON user_devices (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804210035_AddUserDevices') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260804210035_AddUserDevices', '8.0.8');
    END IF;
END $EF$;
COMMIT;

