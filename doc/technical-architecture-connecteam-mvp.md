# Technical Architecture — Deskless Workforce SaaS MVP

> Stack hedefi: Next.js Web Admin + React Native Mobile + .NET 9 Web API + Supabase PostgreSQL + Cloudflare R2 + Upstash Redis + Hangfire + Sentry.

---

## 1. Genel Mimari

```text
                 ┌────────────────────────┐
                 │ Next.js Web Admin       │
                 │ Vercel                 │
                 └───────────┬────────────┘
                             │ HTTPS / JWT
                             ▼
                 ┌────────────────────────┐
                 │ .NET 9 Web API          │
                 │ Render / Docker         │
                 └───────────┬────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        ▼                    ▼                    ▼
┌──────────────┐     ┌────────────────┐    ┌──────────────────┐
│ PostgreSQL   │     │ Redis / Upstash│    │ Cloudflare R2     │
│ Supabase DB  │     │ Cache / Jobs   │    │ File Storage      │
└──────────────┘     └────────────────┘    └──────────────────┘
                             │
                             ▼
                    ┌────────────────┐
                    │ Hangfire        │
                    │ Background Jobs │
                    └────────────────┘
```

---

## 2. Frontend — Next.js Web Admin

### 2.1 Teknoloji

- Next.js App Router
- TypeScript
- Tailwind CSS
- React Hook Form
- Zod
- TanStack Query
- Zustand
- Axios veya fetch wrapper
- Sentry Next.js SDK

---

## 3. Next.js Proje Yapısı

```text
src/
  app/
    (auth)/
      login/
        page.tsx
      register/
        page.tsx
      forgot-password/
        page.tsx
      accept-invite/
        page.tsx

    (dashboard)/
      layout.tsx
      dashboard/
        page.tsx
      employees/
        page.tsx
        [id]/
          page.tsx
      teams/
        page.tsx
      locations/
        page.tsx
      schedule/
        page.tsx
      time-clock/
        page.tsx
      timesheets/
        page.tsx
      tasks/
        page.tsx
        [id]/
          page.tsx
      forms/
        page.tsx
        builder/
          page.tsx
        submissions/
          page.tsx
      announcements/
        page.tsx
      leave/
        page.tsx
      reports/
        page.tsx
      settings/
        page.tsx

  components/
    ui/
    layout/
    forms/
    tables/
    charts/
    modals/

  features/
    auth/
    employees/
    teams/
    locations/
    schedule/
    time-clock/
    tasks/
    forms/
    announcements/
    leave/
    reports/
    settings/

  lib/
    api/
      client.ts
      endpoints.ts
    auth/
      token-storage.ts
      session.ts
    validation/
    utils/

  hooks/
    use-auth.ts
    use-current-user.ts
    use-permissions.ts

  stores/
    auth-store.ts
    organization-store.ts

  types/
    api.ts
    auth.ts
    employee.ts
    shift.ts
    task.ts
```

---

## 4. Next.js Route Mantığı

### Public Routes

```text
/login
/register
/forgot-password
/accept-invite
```

### Protected Routes

```text
/dashboard
/employees
/teams
/locations
/schedule
/time-clock
/timesheets
/tasks
/forms
/announcements
/leave
/reports
/settings
```

### Route Group Yapısı

```text
app/(auth)       -> public layout
app/(dashboard)  -> authenticated admin layout
```

Dashboard layout içinde:

- Sidebar
- Topbar
- Organization switcher
- User menu
- Permission guard

---

## 5. Frontend Auth Akışı

1. Kullanıcı login olur.
2. API access token + refresh token döner.
3. Access token memory/store içinde tutulur.
4. Refresh token httpOnly cookie veya secure storage mantığıyla yönetilir.
5. API wrapper her request'e Bearer token ekler.
6. 401 gelirse refresh denenir.
7. Refresh başarısızsa logout yapılır.

MVP için basit yol:

- Access token: client store
- Refresh token: secure cookie
- Middleware: protected route kontrolü

---

## 6. API Client Yapısı

```ts
src/lib/api/client.ts
```

```ts
export const apiClient = async <T>(
  url: string,
  options?: RequestInit
): Promise<T> => {
  const token = getAccessToken();

  const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}${url}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Authorization: token ? `Bearer ${token}` : '',
      ...options?.headers,
    },
  });

  if (!response.ok) {
    throw new Error('API request failed');
  }

  return response.json();
};
```

---

## 7. Backend — .NET 9 Web API (Clean Architecture)

Bu aşamada sadece iskeleti kuruyoruz. Entity ve business kuralları bir sonraki adımda eklenecek.

### 7.1 Solution Topolojisi

```text
src/
  Workforce.Api/            # Presentation (HTTP)
  Workforce.Application/    # Use case orchestration
  Workforce.Domain/         # Core model (placeholder for now)
  Workforce.Persistence/    # EF Core + PostgreSQL
  Workforce.Infrastructure/ # External services (R2, email, push, Sentry)
  Workforce.Contracts/      # Public request/response contracts
  Workforce.Shared/         # Cross-cutting primitives

tests/
  Workforce.UnitTests/
  Workforce.IntegrationTests/
  Workforce.ArchitectureTests/
```

### 7.2 Proje Bağımlılık Kuralları

```text
Api -> Application
Api -> Contracts
Application -> Domain
Application -> Shared
Infrastructure -> Application
Persistence -> Application
Persistence -> Domain
Infrastructure -> Shared
Persistence -> Shared

Domain -> (no dependency)
Contracts -> (no dependency)
Shared -> (no dependency)
```

Kritik kural: `Application` katmanı `Infrastructure` ve `Persistence` implementasyonlarını bilmez, sadece abstraction bilir.

---

## 8. Katman Sorumlulukları (Saf Mimari)

### Workforce.Api

- Endpoint tanımları (Minimal API veya Controller)
- AuthN/AuthZ middleware pipeline
- Global exception handling
- OpenAPI/Swagger
- Composition root (DI wiring start point)

### Workforce.Application

- Use case akışları (Command/Query)
- Input validation
- Authorization policy check contract'ları
- Transaction boundary abstraction
- Domain event dispatch contract'ları

### Workforce.Domain

- Şimdilik placeholder katman
- Sonraki fazda entity, value object, domain event eklenecek
- Dış teknoloji bağımlılığı olmayacak

### Workforce.Persistence

- EF Core DbContext
- Mapping/configuration
- Migration yönetimi
- Repository ve UnitOfWork implementasyonları

### Workforce.Infrastructure

- Email/push provider adapter'ları
- Object storage adapter'ı (R2)
- Redis adapter'ı
- Sentry/observability adapter'ları
- Hangfire job runner implementasyonları

### Workforce.Contracts

- API request/response contract'ları
- Versionlanabilir DTO sınırı
- Frontend ile paylaşılan stabil sözleşme

### Workforce.Shared

- Result/Error primitive'leri
- Ortak exception tipleri
- Sabitler ve extension metotları

---

## 9. Backend Klasör İskeleti

```text
Workforce.Api/
  Program.cs
  DependencyInjection/
    ServiceRegistration.cs
  Middleware/
    ExceptionHandlingMiddleware.cs
    RequestContextMiddleware.cs
  Endpoints/
    V1/
      AuthEndpoints.cs
      EmployeeEndpoints.cs
  OpenApi/

Workforce.Application/
  DependencyInjection/
    ServiceRegistration.cs
  Abstractions/
    Persistence/
    Identity/
    Storage/
    Notifications/
    Caching/
    Jobs/
    Observability/
  Common/
    Behaviors/
    Exceptions/
    Models/
    Security/
  Features/
    Auth/
      Commands/
      Queries/
    Employees/
      Commands/
      Queries/
    Teams/
      Commands/
      Queries/

Workforce.Domain/
  Primitives/
  Events/
  Specifications/

Workforce.Persistence/
  DependencyInjection/
    ServiceRegistration.cs
  Context/
    ApplicationDbContext.cs
  Configurations/
  Repositories/
  Migrations/

Workforce.Infrastructure/
  DependencyInjection/
    ServiceRegistration.cs
  Storage/
  Notifications/
  Cache/
  Jobs/
  Monitoring/

Workforce.Contracts/
  V1/
    Auth/
    Employees/
    Teams/

Workforce.Shared/
  Results/
  Errors/
  Constants/
  Extensions/
```

---

## 10. Application Pattern (CQRS + Pipeline)

Her use case bir `Command` veya `Query` olarak modellenir.

```text
Features/{Module}/Commands/{UseCase}
  {UseCase}Command.cs
  {UseCase}Validator.cs
  {UseCase}Handler.cs

Features/{Module}/Queries/{UseCase}
  {UseCase}Query.cs
  {UseCase}Handler.cs
```

Not: Bu aşamada sadece klasör ve kontrat iskeleti açılır, iş kuralı kodu yazılmaz.

---

## 11. MediatR Pipeline Sırası

```text
1) Correlation/Logging
2) Validation
3) Authorization
4) Caching (query için opsiyonel)
5) Transaction (command için)
6) Handler
7) Performance/Metrics
```

Hedef: cross-cutting concern'leri handler dışında merkezi yönetmek.

---

## 12. API Surface Standardı

- Versioning: `/api/v1/...`
- Uniform response envelope: success/error formatı sabit
- ProblemDetails: beklenmeyen hatalar için standart çıktı
- Idempotency-Key: kritik POST endpoint'lerde opsiyonel destek
- Pagination contract: `page`, `pageSize`, `totalCount`

Controller/endpoint kuralı: sadece request map et, `ISender` çağır, response dön.

---

## 13. Composition Root ve DI

Her proje kendi registration extension'ını expose eder:

```text
services
  .AddApi()
  .AddApplication()
  .AddPersistence(configuration)
  .AddInfrastructure(configuration);
```

Kural: interface `Application`da, implementation `Persistence/Infrastructure`da.

---

## 14. İlk Kurulum Sprinti (Kod Yazmadan Önce)

1. Solution ve projeleri oluştur.
2. Proje referanslarını dependency kurallarına göre bağla.
3. `ServiceRegistration` dosyalarını ve boş extension metodlarını aç.
4. Global exception middleware ve request context middleware iskeletini ekle.
5. MediatR + FluentValidation pipeline davranışlarını boş handler'larla doğrula.
6. PostgreSQL bağlantısı ve boş migration akışını çalıştır.
7. Health check endpoint'lerini ayağa kaldır (`/health`, `/health/db`, `/health/redis`, `/health/storage`).
8. Architecture test projesinde dependency rule testlerini ekle.

Bu sprintin çıktısı: iş kuralı yazmadan deploy edilebilir, test edilebilir bir backend iskeleti.

---

## 15. Multi-Tenant Mimari

Her tenant = organization.

Tüm ana tablolarda zorunlu alan:

```text
organization_id
```

API her request'te aktif organization context kullanmalı.

```csharp
public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid OrganizationId { get; }
    string Role { get; }
}
```

Query filtreleri organization scope ile çalışmalı.

```csharp
_context.Employees
    .Where(x => x.OrganizationId == currentUser.OrganizationId)
```

Cross-tenant data leak en kritik güvenlik riskidir.

---

## 16. Authentication & Authorization

### Auth

- .NET Identity veya custom user table
- JWT access token
- Refresh token
- Password hashing: BCrypt veya Argon2

### Authorization

- Role based access control
- Organization scoped permissions
- Feature based permission check

Örnek permission:

```text
employees.view
employees.create
employees.update
employees.archive
shifts.create
shifts.publish
time_entries.approve
reports.export
```

---

## 17. PostgreSQL / Supabase DB

Supabase burada backend değil, managed PostgreSQL olarak kullanılacak.

Kullanım:

- PostgreSQL hosting
- Backup
- DB dashboard
- SQL editor

Kullanılmayacak veya dikkatli kullanılacak:

- Supabase Auth
- Direct client-side DB access
- RLS ana authorization olarak

Ana authorization .NET API tarafında kalmalı.

---

## 18. Storage — Cloudflare R2

Dosyalar API üzerinden presigned URL mantığıyla yüklenecek.

### Akış

1. Frontend API'den upload URL ister.
2. API dosya tipi, boyut ve entity permission kontrol eder.
3. API R2 için presigned upload URL üretir.
4. Frontend dosyayı direkt R2'ye yükler.
5. Frontend API'ye `complete-upload` çağrısı yapar.
6. API `files` tablosuna kayıt atar.

### File Table

```text
id
organization_id
uploaded_by
entity_type
entity_id
file_name
mime_type
size_bytes
storage_key
public_url
created_at
```

### Storage Kuralları

- Public bucket kullanma.
- Signed URL kullan.
- MIME type validate et.
- Maksimum dosya boyutu koy.
- Fotoğraf upload için image compression düşün.
- Virus scan Faz 2 olabilir.

---

## 19. Hangfire

Hangfire background işler için kullanılacak.

### Storage

Hangfire için iki seçenek:

1. PostgreSQL storage
2. Redis storage

MVP için PostgreSQL storage yeterli.

### Job Tipleri

```text
InviteEmailJob
ShiftPublishedNotificationJob
ShiftReminderJob
TimesheetReminderJob
MissingClockOutJob
LeaveRequestNotificationJob
ReportExportJob
FileCleanupJob
```

### Dashboard

```text
/admin/jobs
```

Sadece Owner/Admin erişmeli.

### Recurring Jobs

```text
Every 15 minutes -> Missing clock out check
Every hour       -> Shift reminder check
Every day 01:00  -> Old temp file cleanup
Every day 02:00  -> Report aggregation
```

### Job Kuralı

Controller veya handler içinde ağır işlem yapılmamalı.

Örnek:

```csharp
_backgroundJobClient.Enqueue<IShiftNotificationJob>(
    job => job.SendShiftPublishedNotificationAsync(shiftId));
```

---

## 20. Redis / Upstash

Redis kullanım alanları:

- Cache
- Rate limiting
- Distributed lock
- Temporary tokens
- Optional Hangfire storage

MVP cache örnekleri:

```text
current-user-permissions
organization-settings
feature-flags
location-geofence-settings
```

---

## 21. Sentry

Sentry hem frontend hem backend için kullanılacak.

### Next.js

- Client errors
- Server component errors
- API route errors
- Performance tracing

### .NET API

- Unhandled exceptions
- Request tracing
- Background job errors
- User context
- Organization context

### Sentry Context

Her error'a eklenmeli:

```text
user_id
organization_id
role
request_id
environment
app_version
```

### Sensitive Data

Sentry'ye şunlar gönderilmemeli:

- Password
- Token
- Refresh token
- Payment data
- Full address, gerekmedikçe
- Precise GPS coordinates, gerekmedikçe

---

## 22. Logging

Minimum log alanları:

```text
timestamp
level
request_id
user_id
organization_id
method
path
status_code
duration_ms
error_code
```

Öneri:

- Development: console logs
- Production: structured logs
- Error tracking: Sentry

---

## 23. Notification Mimari

### Kanallar

- Push notification: Firebase Cloud Messaging
- Email: Resend / Mailgun / SendGrid
- In-app notification: PostgreSQL table

### Notification Flow

1. Domain event oluşur.
2. Handler notification kaydı oluşturur.
3. Hangfire job push/email gönderir.
4. Başarısız olursa retry edilir.

### Event Örnekleri

```text
EmployeeInvited
ShiftPublished
ShiftUpdated
TaskAssigned
TaskCompleted
LeaveRequested
LeaveApproved
TimesheetRejected
```

---

## 24. Audit Log

Audit log tutulacak işlemler:

- Employee create/update/archive
- Role change
- Shift create/update/delete/publish
- Time entry edit
- Timesheet approve/reject
- Leave approve/reject
- Organization settings update

### Audit Table

```text
id
organization_id
actor_member_id
action
entity_type
entity_id
before_json
after_json
ip_address
user_agent
created_at
```

---

## 25. Deployment

### Frontend

```text
Next.js -> Vercel
```

### Backend

```text
.NET API -> Render
```

Render deployment:

- Dockerfile
- GitHub auto deploy
- Environment variables
- Health check endpoint

### Database

```text
Supabase PostgreSQL
```

### Storage

```text
Cloudflare R2
```

### Redis

```text
Upstash Redis
```

---

## 26. Backend Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish src/Workforce.Api/Workforce.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Workforce.Api.dll"]
```

---

## 27. Environment Variables

### API

```env
ASPNETCORE_ENVIRONMENT=Production
DATABASE_URL=
JWT_SECRET=
JWT_ISSUER=
JWT_AUDIENCE=
REDIS_URL=
R2_ACCOUNT_ID=
R2_ACCESS_KEY_ID=
R2_SECRET_ACCESS_KEY=
R2_BUCKET_NAME=
R2_PUBLIC_URL=
SENTRY_DSN=
FCM_SERVER_KEY=
EMAIL_PROVIDER_API_KEY=
```

### Next.js

```env
NEXT_PUBLIC_API_URL=
NEXT_PUBLIC_SENTRY_DSN=
NEXT_PUBLIC_APP_ENV=production
```

---

## 28. Health Checks

Backend endpoint:

```text
GET /health
GET /health/db
GET /health/redis
GET /health/storage
```

Render health check:

```text
/health
```

---

## 29. MVP Development Order

### Phase 1 — Foundation

- Solution setup
- Next.js app setup
- Auth
- Organization
- Employee management
- RBAC
- PostgreSQL migrations

### Phase 2 — Core Operations

- Locations
- Shift scheduling
- Time clock
- Geofence validation
- Timesheets

### Phase 3 — Workflows

- Tasks
- Forms/checklists
- File upload
- Announcements
- Leave requests

### Phase 4 — Production Ready

- Hangfire jobs
- Sentry
- Audit logs
- Reports CSV
- Rate limiting
- Deployment

---

## 30. Net Tavsiye

Bu proje için en doğru mimari:

```text
Next.js Web Admin
React Native Mobile
.NET 9 Web API
PostgreSQL
Cloudflare R2
Hangfire
Redis
Sentry
```

Supabase sadece PostgreSQL provider gibi kullanılmalı. Ana business logic ve authorization .NET API içinde kalmalı.

MVP'de en kritik şey mimariyi abartmadan temiz başlamak:

- Controller ince
- Business logic Application katmanında
- Entity kuralları Domain içinde
- DB işleri Persistence içinde
- External servisler Infrastructure içinde
- Jobs Hangfire içinde
- Monitoring Sentry ile

