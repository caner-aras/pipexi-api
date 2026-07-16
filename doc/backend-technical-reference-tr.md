# Backend Teknik Referans (TR)

Bu doküman, mevcut backend iskeletinde kullanılan temel bileşenleri tek noktada açıklar:
- Nerede tanımlı oldukları (referans)
- Ne iş yaptıkları
- Neden önemli oldukları

---

## 1) Mimari Stil: Minimal API

Bu projede endpoint'ler controller/action yerine Minimal API yaklaşımıyla map ediliyor.

Ana map noktası:
- `src/Workforce.Api/Program.cs`

Endpoint gruplarının map edildiği çağrılar:
- `app.MapHealthEndpoints();`
- `app.MapAuthEndpoints();`
- `app.MapEmployeeEndpoints();`
- `app.MapInternalAuthEndpoints();` (sadece Development ortamında)

Neden önemli:
- Daha az boilerplate
- Route tanımları daha görünür
- Küçük/orta ölçekli servislerde hızlı ilerleme

---

## 2) Referans Matrisi (Ne, Nerede, Neden)

| Bileşen / Symbol | Referans | Ne İş Yapar | Neden Önemli |
|---|---|---|---|
| `MapHealthEndpoints` | `src/Workforce.Api/Endpoints/V1/HealthEndpoints.cs` | `/health`, `/health/db`, `/health/redis`, `/health/storage` route'larını map eder | Operasyonel sağlık kontrolü (readiness/liveness benzeri) |
| `MapEmployeeEndpoints` | `src/Workforce.Api/Endpoints/V1/EmployeeEndpoints.cs` | Employee domain route grubunu (`/api/v1/employees`) map eder | Employee API yüzeyi tek yerde yönetilir |
| `MapAuthEndpoints` | `src/Workforce.Api/Endpoints/V1/AuthEndpoints.cs` | Auth route grubunu map eder | Auth endpoint düzeni net olur |
| `MapInternalAuthEndpoints` | `src/Workforce.Api/Endpoints/V1/InternalAuthEndpoints.cs` | Internal test token endpointini map eder | QA/test akışında token alma kolaylığı |
| `RequestContextMiddleware` | `src/Workforce.Api/Middleware/RequestContextMiddleware.cs` | `X-Request-Id` üretir/aktarır, `TraceIdentifier` set eder | Log ve hata izleme korelasyonu sağlar |
| `ExceptionHandlingMiddleware` | `src/Workforce.Api/Middleware/ExceptionHandlingMiddleware.cs` | Unhandled exception'ları yakalayıp standart response döner | Tutarlı hata çıktısı + merkezi loglama |
| `AppError` | `src/Workforce.Shared/Errors/AppError.cs` | Uygulama içi standart hata modeli (`Code`, `Message`) | Katmanlar arası hata dilini standartlaştırır |
| `Result` | `src/Workforce.Shared/Results/Result.cs` | İş katmanı başarı/hata temsil modeli | Domain/Application sonucu HTTP'den bağımsız taşır |
| `DomainMarker` | `src/Workforce.Domain/Primitives/DomainMarker.cs` | Assembly referansı için marker class | Architecture testlerinde assembly yakalamayı kolaylaştırır |
| API DI (`AddApi`) | `src/Workforce.Api/DependencyInjection/ServiceRegistration.cs` | JWT, Authorization, HealthChecks, Swagger kayıtlarını yapar | API composition root düzeni |
| App DI (`AddApplication`) | `src/Workforce.Application/DependencyInjection/ServiceRegistration.cs` | MediatR, FluentValidation, pipeline behavior kayıtları | Use-case hattı ve cross-cutting yönetimi |

---

## 3) `sealed`, `static`, Normal Class Ne Zaman?

### `static` class
Örnek kullanım:
- Endpoint map taşıyıcıları (`AuthEndpoints`, `EmployeeEndpoints`, `HealthEndpoints`)

Ne zaman:
- State yoksa
- Sadece yardımcı/extension method varsa

Neden:
- Niyet net olur: instance alınmayacak

### `sealed class`
Örnek kullanım:
- Middleware sınıfları (`RequestContextMiddleware`, `ExceptionHandlingMiddleware`)
- Basit model sınıfları (`Result`)

Ne zaman:
- Kalıtım alınmasını istemiyorsan

Neden:
- Tasarım kararı korunur, yanlış extend engellenir

### Normal `class`
Ne zaman:
- Kalıtım/override gerekebilecekse
- Genişletilebilir tasarım hedefleniyorsa

---

## 4) `Results.Created` vs `Workforce.Shared.Results.Result`

Bunlar aynı seviyede şeyler değildir.

- `Results.Created(...)`: HTTP response üretir (API katmanı).
- `Result`: İş katmanındaki başarı/hata modelidir (Application/Domain odaklı).

Önerilen kullanım:
1. Application katmanı `Result` (veya `Result<T>`) döner.
2. API katmanı bunu HTTP'ye map eder (`Ok`, `Created`, `BadRequest`, `Conflict`, ...).

Neden önemli:
- HTTP detayını iş mantığından ayırır.
- Test edilebilirlik ve katman ayrımı güçlenir.

---

## 5) `Workforce.Contracts` Katmanı Ne İş Yapar?

Katman amacı:
- Dış dünya ile yapılan request/response sözleşmelerini tutar.

Örnek içerik:
- `src/Workforce.Contracts/V1/...`

Neden önemli:
- Domain modeli dışarı sızmaz.
- API versiyonlaması daha temiz yönetilir.
- Frontend/mobil istemciler için stabil contract sağlanır.

---

## 6) Health Endpointlerinin Swagger Görünürlüğü

Durum:
- Health route'ları `MapHealthChecks` ile eklenir.
- Bu endpointler çoğu kurulumda OpenAPI listesine route gibi düşmez.

Bu bir gizleme parametresi değildir:
- `Predicate` sadece hangi checklerin çalışacağını belirler.
- `ResponseWriter` sadece response formatını belirler.

Gerçek gizleme örneği:
- Internal endpointteki `.ExcludeFromDescription()` çağrıları.

---

## 7) Internal Supabase Token Endpointi

Referans:
- `src/Workforce.Api/Endpoints/V1/InternalAuthEndpoints.cs`

Amaç:
- Test/QA için Supabase password grant üzerinden access token alma.

Koruma:
- `X-Internal-Api-Key` header doğrulaması.
- Development ortamında map edilmesi önerilir.

Uyarı:
- Production'da açık bırakılmamalı.
- Mutlaka güçlü API key + ağ/ortam kısıtı uygulanmalı.

---

## 8) Middleware'lerin İşlev Özeti

### `RequestContextMiddleware`
- Gelen `X-Request-Id` varsa kullanır, yoksa üretir.
- Response'a aynı header'ı yazar.
- `HttpContext.TraceIdentifier` set eder.

Önemi:
- Dağıtık izleme, log korelasyonu, incident analizinde kritik.

### `ExceptionHandlingMiddleware`
- Unhandled exception'ı merkezi yakalar.
- Loglar.
- Standart problem response döner.

Önemi:
- API hata davranışı tek tip olur.
- Üretim ortamında debug karmaşasını azaltır.

---

## 9) Kısa Uygulama Rehberi (Employee için)

`MapEmployeeEndpoints` içinde endpoint ekleme akışı:
1. Route group oluştur (`/api/v1/employees`).
2. GET/POST/PUT/DELETE route'larını map et.
3. Handler içinde iş mantığını Application katmanına delege et (MediatR).
4. Dönen sonucu HTTP response'a çevir.

Öneri:
- Endpoint dosyası ince kalsın.
- Validation/authorization pipeline'da çalışsın.

---

## 10) Sonraki İyileştirme Notları

- `Result<T>` eklenmesi (payload + error modelleme için)
- API katmanında `Result -> IResult` mapper extension
- Exception middleware'de RFC7807 `ProblemDetails` standardına tam geçiş
- Internal endpoint için rate limit + allowlist
- Health check'lerde readiness/liveness ayrımı
