# PostgreSQL Migration Rehberi (Pipexi)

Bu dokuman, Pipexi projesinde EF Core migration yonetimi icin adim adim kullanim rehberidir.

## 1. Mimari Ozet

- Persistence katmani EF Core + Npgsql kullanir.
- DbContext sinifi: src/Pipexi.Persistence/Context/ApplicationDbContext.cs
- Ilk migration: src/Pipexi.Persistence/Migrations/20260709130130_CreateOrganizations.cs
- Startup (Development) ortami acilisinda otomatik migration calisir: src/Pipexi.Api/Program.cs

## 2. On Kosullar

- .NET SDK 8.x kurulu olmali.
- PostgreSQL erisimi olmali (Supabase veya local PostgreSQL).
- Connection string `ConnectionStrings:DefaultConnection` altinda tanimli olmali.

Ornek (Development):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Port=5432;Database=Pipexi;Username=...;Password=..."
  }
}
```

## 3. Paketler

Asagidaki paketler projede tanimlidir:

- Pipexi.Persistence.csproj
  - Microsoft.EntityFrameworkCore
  - Microsoft.EntityFrameworkCore.Design
  - Npgsql.EntityFrameworkCore.PostgreSQL
- Pipexi.Api.csproj
  - Microsoft.EntityFrameworkCore.Design (EF tool startup gereksinimi)

## 4. Migration Komutlari

Komutlar repo root altinda calistirilmalidir.

### Yeni migration ekleme

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations add <MigrationName> \
  --project src/Pipexi.Persistence/Pipexi.Persistence.csproj \
  --startup-project src/Pipexi.Api/Pipexi.Api.csproj \
  --context Pipexi.Persistence.Context.ApplicationDbContext \
  --output-dir Migrations

PGPASSWORD='WfMcDe4Pgz38nN#' psql "host=db.niqqiurqcutcanlehsop.supabase.co port=5432 dbname=postgres user=postgres sslmode=require" -c "\dt public.*" -c "select * from \"__EFMigrationsHistory\";"

PGPASSWORD='WfMcDe4Pgz38nN#' psql "host=db.niqqiurqcutcanlehsop.supabase.co port=5432 dbname=postgres user=postgres sslmode=require" -c "select \"MigrationId\" from \"__EFMigrationsHistory\" order by \"MigrationId\";" -c "\dt public.*"


dotnet ef database update --project src/Pipexi.Persistence/Pipexi.Persistence.csproj --startup-project src/Pipexi.Api/Pipexi.Api.csproj --context Pipexi.Persistence.Context.ApplicationDbContext --connection "Host=db.niqqiurqcutcanlehsop.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=WfMcDe4Pgz38nN#;SSL Mode=Require;Trust Server Certificate=true" 

```

Ornek:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations add AddEmployeeTable \
  --project src/Pipexi.Persistence/Pipexi.Persistence.csproj \
  --startup-project src/Pipexi.Api/Pipexi.Api.csproj \
  --context Pipexi.Persistence.Context.ApplicationDbContext \
  --output-dir Migrations
```

### Migrationlari veritabanina uygulama

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update \
  --project src/Pipexi.Persistence/Pipexi.Persistence.csproj \
  --startup-project src/Pipexi.Api/Pipexi.Api.csproj \
  --context Pipexi.Persistence.Context.ApplicationDbContext
```

### Son migrationi geri alma (dosya)

Sadece migration dosyasini geri alir, DB degisikliklerini geri almaz:

```bash
dotnet ef migrations remove \
  --project src/Pipexi.Persistence/Pipexi.Persistence.csproj \
  --startup-project src/Pipexi.Api/Pipexi.Api.csproj \
  --context Pipexi.Persistence.Context.ApplicationDbContext
```

### SQL script uretme

```bash
dotnet ef migrations script \
  --project src/Pipexi.Persistence/Pipexi.Persistence.csproj \
  --startup-project src/Pipexi.Api/Pipexi.Api.csproj \
  --context Pipexi.Persistence.Context.ApplicationDbContext \
  --output migration.sql
```

## 5. Organization Tablosu

Organization entity map'i su dosyadadir:

- src/Pipexi.Persistence/Configurations/OrganizationConfiguration.cs

Olusan tablo ozeti:

- `organizations`
- `id` (uuid, PK)
- `name` (varchar(200), not null)
- `slug` (varchar(200), not null, unique)
- `timezone` (varchar(100), default `UTC`)
- `status` (varchar(30), default `active`)
- `created_at` (timestamp with time zone, not null)
- `updated_at` (timestamp with time zone, null)

## 6. Otomatik Migration Davranisi

Development ortaminda API acilisinda su kod calisir:

- `dbContext.Database.Migrate();`

Avantaj:

- Lokal gelistirmede migration unutulsa bile schema guncel kalir.

Dikkat:

- Production ortaminda otomatik migration tavsiye edilmez.
- Production icin CI/CD adiminda kontrollu `dotnet ef database update` tercih edilmelidir.

## 7. Sorun Giderme

### Hata: startup project EF Design reference istemesi

Belirti:

- "startup project doesn't reference Microsoft.EntityFrameworkCore.Design"

Cozum:

- Startup proje (Pipexi.Api) icine `Microsoft.EntityFrameworkCore.Design` paketini ekleyin.

### Hata: ConnectionStrings:DefaultConnection yok

Belirti:

- Uygulama acilisinda `ConnectionStrings:DefaultConnection is not configured.`

Cozum:

- appsettings.Development.json veya environment variable ile baglanti bilgisini verin.

### Hata: veritabani erisim/izin

Belirti:

- Timeout, auth error, SSL error

Cozum:

- Host/port/user/password degerlerini kontrol edin.
- Supabase tarafinda network whitelist/policy ayarlarini dogrulayin.

## 8. Guvenlik Notu

- Gercek DB sifresini repo icinde tutmayin.
- Tavsiye edilen kullanim:
  - local: `dotnet user-secrets`
  - CI/CD: secret vault veya pipeline secret
  - runtime: environment variable

## 9. Hizli Check List

- [ ] Model degisti mi?
- [ ] Mapping guncellendi mi?
- [ ] `dotnet ef migrations add` calisti mi?
- [ ] `dotnet ef database update` basarili mi?
- [ ] API acilip endpointler test edildi mi?
