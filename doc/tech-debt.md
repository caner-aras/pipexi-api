# Tech debt — manager side

Personel tarafına geçmeden önce özellikle **güvenlik** ve **oturum** maddelerine bak.

## P0 — Güvenlik (önce bunlar)

### API
- [x] Org membership enforce et (login yetmez; “bu org’un üyesi mi?”)
- [x] By-id IDOR kapat (shift / task / team / member / report / delete)
- [x] Path’teki `organizationId` ignore etme (`_ = organizationId` temizle, doğrula)
- [x] Listelerde `organizationId` yoksa tüm kayıtları dönme
- [x] JWT / `/me` üzerinden org + role bilgisini düzgün hydrate et
- [ ] Permission/role check’lerini handler’larda kullan (tablolar var, runtime yok)
- [ ] Supabase RLS: `using (true)` yerine gerçek org-scoped policy (deferred)

## P1 — Oturum

### API + App + Web (aynı sözleşme)
- [x] Refresh token akışı (sakla, 401’de refresh, başarısızsa logout)
- [x] Access token expiry ile cookie/session TTL’ini hizala (web)

### Web
- [x] Auth gate: sadece cookie varlığı değil, token validate
- [x] OAuth / sync path’lerini shared API client ile hizala

### App
- [x] Logout’ta workspace + org/location store temizle
- [x] Create org / location sonrası header cache invalidate

## P2 — API kalite

- [x] `ValidationException` → 400 (şu an 500)
- [x] Report summary: bellek yükü + shift başına N+1 azalt
- [x] Task list/detail hydration tutarlılığı (reporter / assignee / comments)
- [x] Eksik FluentValidation’lar (özellikle delete + day-off + positions)

## P3 — App

- [x] Tema kaynağı tek olsun (`useAppStore.theme`; RN `useColorScheme` / ThemeContext karışımını bitir)
- [x] Tab bar + scene background aynı tema kaynağından
- [x] TanStack Query’yi gerçekten kullan veya provider’ı kaldır
- [x] Focus refetch stratejisini hizala (Dashboard/Teams vs Today/Tasks/Shifts)
- [ ] Login hardcoded credentials kaldır
- [x] Auth dışı formları RHF + Zod’a çek (AGENTS.md)
- [x] `shifts/[shiftId]` local MetaCard → shared `MetaCard`
- [x] Member detail Tasks kartı: tab yoksa onPress kaldır / panel ekle
- [ ] `api.ts` production `console.log` temizle
- [ ] `CustomTabBar` / catch bloklarındaki `any` azalt

## P4 — Web

- [x] Task reporter tipi + UI (app parity)
- [x] Avatar helper + person-colors (app parity)
- [x] BFF ad-hoc fetch → typed service katmanı
- [x] Landing/auth hardcoded renkleri design token’lara bağla
- [x] Dashboard’da gereksiz org-wide task preload’u gözden geçir

## Notlar

- P0 + P1 bitmeden personel tarafına geçme.
- Org-guard önerisi: current user → üye olduğu org’lar → resource.`organizationId` match (tek pipeline).
