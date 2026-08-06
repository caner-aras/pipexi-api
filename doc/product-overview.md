# Pipexi — Ürün Tanımı ve Tasarım Kılavuzu (Product Overview & AI Design Prompt Doc)

> **Amaç:** Bu doküman, **Pipexi** uygulamasının ne iş yaptığını, modüllerini, kullanıcı rollerini ve ekran akışlarını özetler. AI tasarım araçlarına (Midjourney, v0, Galileo AI, Figma AI vb.) girdi sağlamak üzere hazırlanmıştır.

---

## 🚀 1. Ürün Özeti (Product Summary)

**Pipexi**, sahada ve masa başında çalışan personeller ile yöneticileri (İşletme Sahipleri, İK, Vardiya Amirleri) tek bir platformda buluşturan modern bir **İş Gücü & Operasyon Yönetimi (Workforce & Operations Management SaaS)** mobil ve web uygulamasıdır.

Uygulama; vardiya planlaması, canlı zaman takibi (Giriş/Çıkış), görev atama, şirket içi mesajlaşma, saha formları ve izin yönetimini tek bir çatı altında toplar.

---

## 👥 2. Kullanıcı Rolleri ve Hedef Kitle (User Roles)

Uygulama iki temel kullanıcı görünümüne ayrılır:

### A. Yönetici & İşletme Sahibi (Owner / Manager / Admin)
- **Ekran Yolu:** `/(owner)/(tabs)`
- **Hedef:** Operasyonu canlı izlemek, acil aksiyon sinyallerini görmek, vardiya ve görev atamak, izin onaylamak, raporları analiz etmek.

### B. Saha Çalışanı & Personel (Member / Employee / Staff)
- **Ekran Yolu:** `/(member)/(tabs)`
- **Hedef:** Vardiyalarını görmek, tek tıkla mesaiye girmek (Clock-In / Clock-Out), atanan görevleri tamamlamak, sohbet etmek ve izin talebinde bulunmak.

---

## 📦 3. Ana Modüller ve Ekran Detayları (Core Modules & Screens)

### 1. Dashboard & Canlı Operasyon (Home / Dashboard)
- **Live Operations Summary:** O an vardiyada aktif çalışan sayısı, bugün süresi geçen görevler ve doldurulan formların canlı özeti.
- **Action Signals (Aksiyon Sinyalleri):** Müdahale gerektiren acil konuların (Gecikmiş görevler, eksik saha formları, izin talepleri) uyarı rozetleriyle gösterildiği canlı listeleme.
- **Pulse Stats:** Şirket genelindeki aktif personel, tamamlanan vardiya ve görev sayıları.
- **Takvim Cetveli (Schedule Feed):** Günlük/Haftalık bazda personelin vardiya dağılımı.

### 2. Vardiya & Zaman Takibi (Shifts & Time Clock)
- **Vardiya Planlayıcı:** Lokasyon ve pozisyon bazlı haftalık vardiya takvimi.
- **Zaman Çarkı (Time Clock):** Personelin lokasyon bazlı mesaiye giriş/çıkış yaptığı canlı zaman sayacı.
- **Eksik Formlar (Missing Forms):** Vardiya sonunda doldurulması zorunlu olan saha raporları ve formları.

### 3. Görev Yönetimi (Tasks & Subtasks)
- **Görev Kartları:** Görev başlığı, atanan kişiler, öncelik etiketi (`🚨 URGENT`, `⚠️ HIGH`, `MEDIUM`, `LOW`), son teslim tarihi.
- **Görev Detayı & Yorumlar:** Görev içi mesajlaşma, dosya/fotoğraf ekleme, durum değiştirme (`To Do`, `In Progress`, `Completed`).
- **Push Bildirimler:** Görev atandığında veya göreve yorum yazıldığında FCM üzerinden anlık bildirim ve derin link (Deeplink).

### 4. İletişim & Mesajlaşma (Chat & Announcements)
- **Birebir & Grup Sohbetleri (Direct & Group Messages):** Çalışanlar ve yöneticiler arası anlık mesajlaşma.
- **Akıllı Bildirim (15-min Cooldown):** Mesajlaşma sırasında kullanıcıyı spam yapmayan 15 dakikalık bildirim penceresi.
- **Duyurular (Announcements):** Tüm şirkete veya belirli lokasyona yayınlanan resmi duyuru kartları.

### 5. İzin & Saha Formları (Leaves & Forms)
- **İzin Talepleri (Pending Day-offs):** Çalışanların yıllık izin / mazeret izni talepleri ve yöneticinin onay/red ekranı.
- **Dinamik Saha Formları:** Temizlik denetimi, envanter sayımı, açılış/kapanış kontrol listeleri.

---

## 🎨 4. AI Tasarım Robotları İçin Prompt Kılavuzu (AI Design Context)

Bu bilgileri bir AI UI/UX tasarım aracına (v0, Figma AI, Claude Artefacts vb.) girmek istersen aşağıdaki özet promptu kullanabilirsin:

```text
Create a modern, high-end React Native mobile app UI for "Pipexi", a Workforce & Operations Management app.
Design 4 main tab views:

1. Dashboard (Home):
   - Executive header with organization selector and avatar
   - Live Operations Summary card (Active Now, Overdue Tasks, Forms Submitted)
   - Action Signals list with status badges and indicators
   - Interactive weekly shift schedule feed

2. Tasks Tab:
   - Task list filtered by status (To Do, In Progress, Completed)
   - High-contrast priority tags (URGENT red, HIGH orange)
   - Task detail modal with comment thread & activity timeline

3. Shifts / Time Clock:
   - Big, prominent "Clock In / Clock Out" button with active timer
   - Weekly shift roster with member avatars and position tags

4. Chat & Notifications:
   - Direct message list with unread badges
   - Real-time notification center with quick action links

Aesthetic Style: Clean luxury modern UI, high contrast typography, dark/light mode support, subtle borders, card elevation, vibrant action badges, tailored HSL color palettes.
```

---

## 📁 5. Proje Dosya Yapısı Referansı

- **Mobile App:** `app/src/app/(owner)` & `app/src/app/(member)`
- **Components:** `app/src/components/dashboard/`, `app/src/components/tasks/`, `app/src/components/chat/`
- **Backend API:** `.NET 9 Web API` (`Pipexi.Application`, `Pipexi.Domain`, `Pipexi.Infrastructure`)
