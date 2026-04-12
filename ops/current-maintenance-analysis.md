# BeachGo Current Maintenance Analysis

Arşiv tarihi: 2026-04-12
Kaynaklar:
- `Beachgo-Claude-Analiz.txt`
- mevcut repo kodu
- son uygulanan fix'ler:
  - `cfc805a` `fix: avoid timestamptz date grouping in business stats`
  - `21cc63e` `fix: preserve public beach detail access on stale sessions`

## Executive Summary

Repo önceki duruma göre anlamlı şekilde toparlanmış. Public beach detail stale-session redirect problemi ve business stats `timestamptz` hatası çözülmüş durumda. Ancak production'a çıkışı hâlâ durduran iki net konu var:

1. Guest reservation fiyat hesaplaması hâlâ mock.
2. Auth rate limiter hâlâ global bucket kullanıyor, per-IP değil.

Current state'te ayrıca birkaç yüksek öncelikli correctness/security açığı var:
- access token localStorage'da tutuluyor
- guest email verification doğrulaması expiry nedeniyle yanlış negatif üretebilir
- guest reservation beach lookup `FindAsync` ile soft-delete bypass edebilir

Ship edilebilirlik:
- `Features:UseRealPayment=false` iken rezervasyon akışı 503 döner.
- `UseRealPayment=true` açıldığında guest pricing bug yüzünden yanlış tutar üretme riski var.
- Bu nedenle repo şu an güvenle production-ready değil.

## Current-State Validation Notes

Current repo ile yeniden doğrulanan ve artık aktif görünmeyen maddeler:

- `/api/business/stats` 500:
  - `BusinessService.GetStatsAsync()` fixlenmiş.
  - SQL tarafındaki `ReservationDate.Date` grouping kaldırılmış.
- `/beaches/:id` hard refresh `/login` redirect:
  - `AuthContext` bootstrap fixi ile stale localStorage user artık sahte authenticated state oluşturmuyor.
- `/beaches` empty bug:
  - current state'te reproduce olmadı.
  - canlı `GET /api/Beaches` çağrısı dolu veri döndürüyor.
  - frontend route/render current durumda çalışıyor.

Bu yüzden aşağıdaki faz planına eski ama artık kapanmış bulgular dahil edilmedi.

## Findings

### F-01 Guest Reservation Price Calculation Mock
- Severity: blocker
- Status: confirmed
- Area: backend
- Evidence:
  - `BeachRehberi.API/BeachRehberi.API/Services/GuestReservationService.cs`
  - `CalculatePrice()` sadece `EntryFee * personCount` yapıyor.
  - Kod içinde açıkça `Mock fiyat hesaplama — gerçek implementasyon ileride` notu var.
  - `SunbedCount` guest flow'da `0` hardcode ediliyor.
- Why it matters:
  - Gerçek ödeme açıldığında misafir rezervasyonları yanlış fiyatla oluşturulur.
  - Stripe'a yanlış tutar gönderilebilir.
- Smallest safe fix direction:
  - `ReservationService` içindeki server-side fiyat mantığını ortak bir helper'a taşı.
  - `GuestReservationService` bunu kullansın.
  - Guest flow'da `SunbedCount` kasıtlı olarak `0` ise bunu açıkça sabitle ve test et.

### F-02 Auth Rate Limiter Global, Per-IP Değil
- Severity: blocker
- Status: confirmed
- Area: backend / auth
- Evidence:
  - `BeachRehberi.API/BeachRehberi.API/Program.cs`
  - `options.AddFixedWindowLimiter("auth", ...)`
  - `AuthController` üzerinde `[EnableRateLimiting("auth")]`
- Why it matters:
  - Tek bir kullanıcı/IP tüm auth trafiğini aynı global bucket üzerinden tüketebilir.
  - Login/register/refresh hattı topluca kilitlenebilir.
- Smallest safe fix direction:
  - `auth` limiter'ını `AddPolicy("auth", ...)` + `RateLimitPartition.GetFixedWindowLimiter(...)` ile per-IP yap.
  - Controller attribute'ünü aynı bırak.

### F-03 Access Token localStorage'da Persist Ediliyor
- Severity: high
- Status: confirmed
- Area: frontend / auth
- Evidence:
  - `beach-ui/src/api/token.ts`
  - `accessTokenMemory` ve `refreshTokenMemory` localStorage'dan hydrate ediliyor.
  - `setAccessToken()` localStorage'a yazıyor.
- Why it matters:
  - XSS durumunda access token çalınabilir.
  - Refresh cookie kullanılırken access token'ı persistent saklamak gereksiz risk.
- Smallest safe fix direction:
  - Access token'ı memory-only tut.
  - Refresh cookie ile bootstrap akışını koru.
  - Bu değişikliği yapmadan önce refresh flow'u smoke test ile doğrula.

### F-04 Email Verification Expiry False Negative
- Severity: high
- Status: confirmed
- Area: backend / auth
- Evidence:
  - `BeachRehberi.API/BeachRehberi.API/Services/OtpService.cs`
  - `IsEmailVerifiedAsync()` içinde:
    - `x.IsUsed`
    - `x.ExpiresAt > DateTime.UtcNow`
- Why it matters:
  - Kullanıcı OTP'yi başarıyla doğrulasa bile kısa süre sonra rezervasyon sırasında yeniden "doğrulanmamış" görünebilir.
  - Bu guest reservation akışını bozar.
- Smallest safe fix direction:
  - `IsEmailVerifiedAsync()` içindeki expiry şartını kaldır.
  - Doğrulanmış kayıt için `IsUsed` yeterli kabul edilsin.

### F-05 Guest Reservation Beach Lookup Soft-Delete Bypass Riski
- Severity: high
- Status: likely
- Area: backend
- Evidence:
  - `BeachRehberi.API/BeachRehberi.API/Services/GuestReservationService.cs`
  - `var beach = await _db.Beaches.FindAsync(dto.BeachId);`
  - Aynı repo içinde user reservation flow `FirstOrDefaultAsync(... && !b.IsDeleted)` kullanıyor.
- Why it matters:
  - Soft-deleted beach için misafir rezervasyonu kabul edilebilir.
  - Bu veri tutarsızlığı yaratır.
- Smallest safe fix direction:
  - `FindAsync` yerine `FirstOrDefaultAsync(b => b.Id == dto.BeachId && !b.IsDeleted)` kullan.

### F-06 Stripe Webhook Idempotency ve Reservation Status Eksik
- Severity: high
- Status: confirmed
- Area: backend / payments
- Evidence:
  - `BeachRehberi.API/BeachRehberi.API/Controllers/StripeWebhookController.cs`
  - `HandleCheckoutSessionCompletedAsync()` her event'te `reservation.PaymentStatus = Paid` yazar.
  - Aynı method reservation status'unu `Approved`'a çekmiyor.
- Why it matters:
  - Aynı Stripe event birden fazla kez gelirse gereksiz tekrar işlem olur.
  - Ödeme alınmış rezervasyon `Pending` kalabilir.
- Smallest safe fix direction:
  - `if (reservation.PaymentStatus == PaymentStatus.Paid) return;`
  - başarılı ödeme sonrası reservation status'u `Approved` yap.

### F-07 SearchAsync PostgreSQL Index Dostu Değil
- Severity: medium
- Status: confirmed
- Area: backend / db
- Evidence:
  - `BeachRehberi.API/BeachRehberi.API/Services/BeachService.cs`
  - `b.Name.ToLower().Contains(query.ToLower())`
  - `b.Description.ToLower().Contains(query.ToLower())`
- Why it matters:
  - Veri büyüdükçe full scan benzeri maliyet yaratır.
- Smallest safe fix direction:
  - `EF.Functions.ILike` kullan.

### F-08 Business Reservations Query Ağır ve Sayfalamasız
- Severity: medium
- Status: confirmed
- Area: backend / db
- Evidence:
  - `BeachRehberi.API/BeachRehberi.API/Services/BusinessService.cs`
  - `GetAllReservationsAsync(int beachId)` tüm rezervasyonları sayfalamasız çekiyor.
  - Projection içinde correlated subquery kullanıyor:
    - `VerificationCodes.Any(...)`
    - `VerificationCodes.Where(...).Max(...)`
    - `ReservationPayments.Where(...).FirstOrDefault()`
- Why it matters:
  - Rezervasyon sayısı büyüdükçe dashboard latency artar.
- Smallest safe fix direction:
  - Önce pagination ekle.
  - Sonra projection'i gerekirse ikinci adımda optimize et.

### F-09 Redis Compose'ta Var, Kodda Yok
- Severity: medium
- Status: confirmed
- Area: deployment
- Evidence:
  - `docker-compose.yml` redis servisi ve `BEACHGO_REDIS_CONN` env var'ı var.
  - `Program.cs` yalnızca `AddMemoryCache()` kullanıyor.
  - repo içinde aktif Redis cache entegrasyonu yok.
- Why it matters:
  - Compose karmaşıklaşıyor.
  - Multi-instance deployment varsayımı yanlış güven hissi yaratır.
- Smallest safe fix direction:
  - Redis'i opsiyonel profile yap veya compose'dan çıkar.
  - Dağıtık blacklist gerçekten isteniyorsa ayrı görev olarak ele al.

### F-10 PROJECT_NOTES Yanıltıcı Multi-Tenant Dokümantasyon İçeriyor
- Severity: medium
- Status: confirmed
- Area: docs
- Evidence:
  - `docs/PROJECT_NOTES.md`
  - Mevcut `BeachDbContext` içinde olmayan `_currentUserService`, `TenantId`, `SaveChangesAsync` tenant atama davranışı varmış gibi anlatılıyor.
- Why it matters:
  - Başka agent/geliştirici yanlış mimari varsayımıyla ilerler.
- Smallest safe fix direction:
  - Dosyayı current-state'e göre düzelt.
  - Planlanan mimariyse açıkça `[PLANNED]` etiketiyle ayır.

### F-11 E2E Kapsamı Gerçek Integration Güvencesi Vermiyor
- Severity: low
- Status: confirmed
- Area: tests
- Evidence:
  - Claude raporundaki notlarla uyumlu şekilde mevcut test yaklaşımı frontend ağırlıklı.
  - Repo içinde backend + frontend + payment + auth gerçek uçtan uca smoke zinciri kurulu değil.
- Why it matters:
  - CI yeşil olsa da kritik akışlar kırık kalabilir.
- Smallest safe fix direction:
  - UI-only testlerle integration testleri ayır.
  - En azından blocker flow'lar için focused smoke checklist yaz.

## Already Fixed / Not Reproducible

### Fixed
- Business stats `DateTimeKind` / `timestamptz` hatası
  - `cfc805a`
- Public beach detail stale-session redirect
  - `21cc63e`
- Root `.env` ve QA artefact'larının kazara commit edilmesi
  - `.gitignore` güncel

### Not Reproducible in Current State
- `/beaches` empty result
  - canlı `GET /api/Beaches` dolu veri döndürüyor
  - current frontend route render oluyor
- `/beaches/:id` route protected mı?
  - hayır, public route
  - önceki redirect problemi route protection değil bootstrap/auth side-effect idi ve fixlendi

## Completed Since This Analysis

Asagidaki maddeler bu analiz cikarildiktan sonra repository icinde uygulanmistir:

- `8dbe70d` `fix: close auth reservation and ops hardening phase`
  - F-01 guest reservation pricing server-side helper'a tasindi
  - F-02 auth rate limiter per-IP policy oldu
  - F-03 access token persistence memory-only modele cekildi
  - F-04 OTP verification expiry false negative kapatildi
  - F-05 guest reservation soft-delete beach check eklendi
  - F-06 Stripe webhook idempotency + reservation status update eklendi
  - F-07 search query Npgsql `ILike` kullaniyor
  - F-09 compose icinden aktif kullanilmayan Redis cikarildi
  - F-10 `docs/PROJECT_NOTES.md` current-state'e gore yeniden yazildi
  - focused smoke checklist eklendi
- `61af9d3` `feat: move business reservations to server pagination`
  - F-08 business reservations endpoint'i server-side search/filter/sort/page destekli paged response donuyor
  - business dashboard reservations ekrani backend pagination'i gercekten tuketiyor

Bu nedenle asagidaki faz plani tarihsel kayit niteligindedir; bu maddelerin buyuk kismi artik kapatilmis kabul edilmelidir.

## Historical Phase Plan

### Phase 1 — Release Blockers
Goal:
- Gerçek ödeme açılabilir hale gelsin
- Auth hattı tek kullanıcıyla global kilitlenmesin

Tasks:
1. F-01 Guest reservation pricing'i gerçek server-side hesaplamaya geçir
2. F-02 Auth rate limiter'ı per-IP policy yap

Dependency order:
1. F-01
2. F-02

Executor notes:
- F-01: güçlü model / dikkatli review
- F-02: Gemini uygun

Verification:
- backend build
- backend tests
- focused guest reservation pricing tests
- auth limiter smoke check

### Phase 2 — Correctness / Security
Goal:
- Meşru kullanıcıların rezervasyon akışı yanlış negatifler yüzünden kırılmasın
- ödeme callback davranışı güvenli olsun
- session/token yüzeyi küçülsün

Tasks:
1. F-06 Stripe webhook idempotency + status update
2. F-04 OTP verification expiry false negative fix
3. F-05 Guest reservation beach soft-delete check
4. F-03 access token'ı memory-only yap

Dependency order:
1. F-06
2. F-04
3. F-05
4. F-03

Executor notes:
- F-06: güçlü model / dikkatli review
- F-04: Gemini uygun
- F-05: Gemini uygun
- F-03: güçlü model, çünkü auth bootstrap ve refresh davranışı etkileniyor

Verification:
- backend build/tests
- frontend build
- login -> refresh -> page reload smoke
- guest reservation smoke
- Stripe webhook focused smoke

### Phase 3 — Maintainability / Performance
Goal:
- Sorgu maliyetlerini ve yanlış mimari sinyallerini azalt

Tasks:
1. F-07 SearchAsync `ILike`
2. F-08 Business reservations pagination
3. F-09 Redis compose cleanup
4. F-10 PROJECT_NOTES current-state düzeltmesi

Dependency order:
1. F-07
2. F-08
3. F-09
4. F-10

Executor notes:
- F-07: Gemini uygun
- F-08: güçlü model daha iyi
- F-09: Gemini uygun
- F-10: Gemini uygun

Verification:
- backend build/tests
- frontend build
- search smoke
- business reservations UI smoke

### Phase 4 — Tests / Docs / Cleanup
Goal:
- kazanımları kalıcı hale getir

Tasks:
1. F-11 focused smoke / integration checklist veya test ekle
2. blocker flow'lar için eksik unit/integration testleri tamamla
3. docs ve ops notlarını güncelle

Dependency order:
1. test boşluklarını blocker fazlarına göre doldur
2. docs cleanup

Executor notes:
- Gemini küçük test/doc işlerinde uygun
- karmaşık test harness gerekiyorsa güçlü model daha iyi

Verification:
- lint
- typecheck
- tests
- build

## Verification Checklist

Backend:
- `dotnet build BeachRehberi.API\\BeachRehberi.API.sln`
- `dotnet test BeachRehberi.API\\BeachRehberi.API.Tests\\BeachRehberi.API.Tests.csproj`

Frontend:
- `npm --prefix beach-ui run build`
- gerekiyorsa `npm --prefix beach-ui run lint`
- gerekiyorsa `npm --prefix beach-ui run typecheck`

Focused smokes:
- business login
- guest reservation create
- Stripe webhook completed/failure
- public beach detail hard refresh
- business reservations dashboard load

## Recommended Next Move

Bu dosyadaki ilk faz gorevleri buyuk olcude tamamlandigi icin bir sonraki ajan once current repo durumunu yeniden degerlendirmeli ve yeni bir gap listesi cikarmalidir.

Uygulama sırası için en mantıklı başlangıç:
1. Phase 1 / F-01
2. Phase 1 / F-02

Bu iki görev kapatılmadan ödeme açmak ve production güveni oluşturmak doğru değil.
