# Next.js Uzerinden Google Auth - Yapilacaklar

Bu dokuman, Google OAuth akisinin Next.js tarafinda kurulmasi ve backendde sadece profil senkronizasyonu kullanilmasi icin bir uygulama listesidir.

## Hedef Mimari

- Google login islemi Next.js tarafinda Supabase istemcisi ile baslatilir.
- Supabase, Google ile OAuth akislarini yonetir.
- Kullanici session/JWT token ile backend API'ye gider.
- Backend token dogrulamanin yaninda local `users` tablosu icin profil senkronizasyonu yapar.

## 1) Supabase Ayarlari

- Supabase Dashboard -> Authentication -> Providers -> Google provider ac.
- Google Client ID ve Client Secret degerlerini gir.
- Authentication -> URL Configuration altinda Site URL ayarla.
- Redirect URL listesine asagidaki callback adresini ekle:
	- local: `http://localhost:3000/auth/callback`
	- prod: `https://senin-domainin.com/auth/callback`

## 2) Google Cloud Console Ayarlari

- Google Cloud Console -> APIs & Services -> Credentials.
- OAuth 2.0 Client olustur (Web application).
- Authorized redirect URI listesine Supabase callback adresini ekle:
	- `https://niqqiurqcutcanlehsop.supabase.co/auth/v1/callback`
- Gerekirse test user ekle (OAuth consent ekraninda).

## 3) Next.js Environment Degiskenleri

- `NEXT_PUBLIC_SUPABASE_URL`
- `NEXT_PUBLIC_SUPABASE_ANON_KEY`
- Gerekirse backend cagri baz URL degiskeni (`NEXT_PUBLIC_API_BASE_URL` gibi)

## 4) Next.js - Supabase Client Kurulumu

- Browser tarafinda kullanacagin supabase client dosyasi olustur.
- Login butonunda `signInWithOAuth({ provider: "google" })` kullan.
- `redirectTo` degerini callback route'una ver.

## 5) Next.js - Callback Route

- `auth/callback` sayfasi/route'u ekle.
- Supabase session bilgisini callbackte al.
- Session varsa kullaniciyi dashboard'a yonlendir.
- Session yoksa login sayfasina geri yonlendir.

## 6) Register/Login Ayrimi (Gerekli)

- Google login sonrasi backend profil sync cagrisini mutlaka calistir.
- Bu cagri ilk login'de local `users` kaydi olusturur, sonraki login'lerde profili gunceller.

## 7) Backend Tarafi (Minimum)

- JWT validate zaten varsa degistirme.
- Yeni `api/v1/auth/google/start` veya `api/v1/auth/google/callback` endpointi ekleme.
- Profile sync endpointi tut:
	- endpoint: `POST /api/v1/auth/sync`
	- amac: Supabase `sub` ile local user kaydi esleme/olusturma
	- auth: `Authorization: Bearer <access_token>` gerekli
	- request body (opsiyonel alanlar): `email`, `firstName`, `lastName`, `phone`, `avatarUrl`
	- response: `created = true` ise yeni kayit acildi, `created = false` ise mevcut kayit guncellendi

## 8) Guvenlik Kontrolleri

- Frontendde token'lari localStorage yerine guvenli cookie/session stratejisiyle yonet.
- Backendde `aud`, `iss`, `exp` kontrolleri aktif olsun.
- CORS sadece gerekli originlerle sinirli olsun.

## 9) Test Senaryolari

- Yeni Google kullanicisi ilk login -> profile olusuyor mu?
- Mevcut kullanici tekrar login -> duplicate kayit olusmuyor mu?
- Login iptal edildi -> kullanici dogru hata/yonlendirme goruyor mu?
- Expired token ile API -> 401 donuyor mu?

## 10) Deploy Checklist

- Production redirect URL'leri Google ve Supabase tarafinda birebir esit.
- Production env degiskenleri dogru.
- Domain/https zorunlulugu dogrulandi.

## Not

Bu yaklasimla OAuth akisinin sahibi Next.js olur. Backend sade kalir ve kimlik dogrulama sonrasinda sadece yetkilendirme/is kurali katmani olarak calisir.