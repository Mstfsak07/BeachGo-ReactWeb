# BeachRehberi API — Project Notes

Arsivlenme tarihi: 2026-04-12

Bu dosya current-state teknik notudur. Repo icinde yer alan eski multi-tenant notlari ve archive parcalari aktif calisma davranisi olarak kabul edilmemelidir.

## Current Runtime Path

- Aktif HTTP yurutme yolu `BeachRehberi.API/Features/` ve `BeachRehberi.API/Services/` kombinasyonudur.
- `BeachRehberi.Application/` altinda ortak tipler bulunur, ancak aktif HTTP handler kaynagi olarak alinmamaktadir.
- MediatR kaydi API assembly uzerinden yapilir.

## Data And Persistence

- Runtime veritabani PostgreSQL'dir.
- API startup tarafinda `BEACHGO_DB_CONN` veya `ConnectionStrings:DefaultConnection` okunur.
- Cache current state'te process-local `AddMemoryCache()` ile calisir.
- `docker-compose.yml` current state'te yalnizca `db`, `api` ve `ui` servislerini tanimlar; aktif Redis baglantisi beklenmez.

## Auth Runtime Notes

- Refresh token akisi backend tarafinda `HttpOnly` cookie ile desteklenir.
- Frontend access token'i persistent storage'da tutmaz; uygulama acilisinda refresh endpoint'i ile yeniden hydrate olur.
- Auth endpoint rate limit policy'si per-IP partition mantigiyla calisir.

## Reservation And Payment Notes

- Reservation fiyati backend tarafinda hesaplanir.
- Guest reservation akisi da ayni server-side pricing helper'ini kullanir.
- Stripe webhook `checkout.session.completed` olayinda tekrar eden paid event'leri no-op kabul eder.
- Basarili odeme sonrasinda pending reservation durumu `Approved`'a cekilir.

## Search And Listing Notes

- Beach search PostgreSQL ortaminda `EF.Functions.ILike` kullanir.
- Test ortaminda InMemory provider nedeniyle case-insensitive fallback sorgusu kullanilir.
- Business reservations endpoint'i liste donmeye devam eder, ancak default olarak sayfalanir.

## Removed Or Inactive Architecture Notes

Asagidaki maddeler repo icinde eski notlar veya archive icerigi olarak bulunabilir, fakat current-state davranisi degildir:

- global multi-tenant query filter'lar
- `_currentUserService` tabanli tenant izolasyonu
- `SaveChangesAsync` icinde otomatik `TenantId` atamasi

Bu basliklar yalnizca tarihsel baglamdir; yeni gelistirmelerde referans mimari olarak alinmamalidir.
