# Audience Rehberi

Bu dokuman Announcement olustururken kullanilan audienceType ve audienceId alanlarini aciklar.

## Alanlar

### audienceType
Duyurunun hedef kitlesinin turudur.

Onerilen degerler:
- all
- team
- member
- role

Not:
- Mevcut kod yapisinda audienceType teknik olarak serbest metin kabul eder.
- Tutarlilik icin yukaridaki degerleri standart olarak kullanin.

### audienceId
Hedef kitlenin kayit ID degeridir.

Kurallar:
- audienceType = all ise audienceId null olmalidir.
- audienceType = team ise audienceId ilgili Team Id olmalidir.
- audienceType = member ise audienceId ilgili OrganizationMember Id olmalidir.
- audienceType = role ise audienceId ilgili Role Id olmalidir.

## Swagger Icin Ornek JSON

### 1) Tum organizasyona duyuru (all)

```json
{
  "organizationId": "bb4efc91-1a23-4f74-90ec-945c7a01b5a9",
  "title": "Yarin ofis kapali",
  "body": "Bakim nedeniyle yarin ofis kapali olacaktir.",
  "audienceType": "all",
  "audienceId": null,
  "publishedAt": "2026-07-10T17:25:30.621Z"
}
```

### 2) Belirli bir takima duyuru (team)

```json
{
  "organizationId": "bb4efc91-1a23-4f74-90ec-945c7a01b5a9",
  "title": "Destek ekibi toplantisi",
  "body": "Saat 10:00 toplantı odasi A",
  "audienceType": "team",
  "audienceId": "11111111-2222-3333-4444-555555555555",
  "publishedAt": "2026-07-10T17:25:30.621Z"
}
```

### 3) Belirli bir uye icin duyuru (member)

```json
{
  "organizationId": "bb4efc91-1a23-4f74-90ec-945c7a01b5a9",
  "title": "Egitim hatirlatmasi",
  "body": "Guvenlik egitimini bugun tamamlayin.",
  "audienceType": "member",
  "audienceId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
  "publishedAt": "2026-07-10T17:25:30.621Z"
}
```

### 4) Belirli bir role duyuru (role)

```json
{
  "organizationId": "bb4efc91-1a23-4f74-90ec-945c7a01b5a9",
  "title": "Yonetici onay hatirlatmasi",
  "body": "Bekleyen izin taleplerini gun sonuna kadar onaylayin.",
  "audienceType": "role",
  "audienceId": "99999999-8888-7777-6666-555555555555",
  "publishedAt": "2026-07-10T17:25:30.621Z"
}
```

## Hızlı Kontrol Listesi

Swagger gonderimi oncesi:
- organizationId dogru organization kaydina ait mi?
- audienceType degeri standartlardan biri mi?
- audienceType all degilse audienceId dolu mu?
- publishedAt UTC formatinda mi?
