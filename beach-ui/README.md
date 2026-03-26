# 🏖️ Beach Rehberi - React Frontend

Backend'e tam uyumlu React frontend. Tüm API endpoint'leri entegre edilmiştir.

## Kurulum

```bash
npm install
npm start
```

Uygulama: http://localhost:3000  
Backend: http://localhost:5143/api

---

## Sayfalar

| Sayfa | URL | Açıklama |
|-------|-----|----------|
| Ana Sayfa | `/` | Hava durumu + plaj listesi + arama |
| Tüm Plajlar | `/beaches` | Filtreli liste (imkân, fiyat, doluluk) |
| Plaj Detay | `/beach/:id` | Detay, hava, rezervasyon, yorum, etkinlik |
| Etkinlikler | `/events` | Bugün + yaklaşan etkinlikler |
| Rezervasyonum | `/reservation-check` | Kod veya telefon ile sorgula / iptal et |
| İşletme Girişi | `/login` | JWT login |
| İşletme Paneli | `/business` | Dashboard (JWT korumalı) |

---

## İşletme Paneli Özellikleri

- 📊 Canlı doluluk güncelleme (slider)
- ✨ Günlük özel mesaj
- 📅 Rezervasyon listesi (tarihe göre)
- 🎉 Etkinlik ekleme / silme

---

## API Servisleri (`src/services/api.js`)

Tüm endpoint'ler hazır:
- `getBeaches()`, `getBeach(id)`, `searchBeaches(q)`, `filterBeaches(filter)`
- `getBeachWeather(id)`, `getKonyaaltiWeather()`
- `login(email, password)`
- `getReviews(beachId)`, `createReview(data)`
- `createReservation(data)`, `getReservationByCode(code)`, `cancelReservation(code)`
- `getEvents()`, `getTodayEvents()`, `getBeachEvents(beachId)`
- `getDashboard()`, `updateOccupancy(percent)`, `updateSpecial(msg)`, `addEvent(data)`, `deleteEvent(id)`

---

## Backend Port Değiştirme

`src/services/api.js` dosyasında:
```js
baseURL: "http://localhost:5143/api"
```
