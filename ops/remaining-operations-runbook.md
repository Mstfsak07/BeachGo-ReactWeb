# Remaining Operations Runbook

## 0. Completed Work Log

Bu repo icinde asagidaki fazlar uygulanip commitlenmistir. Diger bir makinede devam edecek ajan bunlari yeniden planlamamali:

- `8dbe70d` `fix: close auth reservation and ops hardening phase`
- `61af9d3` `feat: move business reservations to server pagination`

Bu commitlerle kapanan basliklar:

- guest reservation pricing server-side hesaplama
- auth per-IP rate limiter
- access token memory-only persistence
- OTP verification expiry false negative
- guest reservation soft-delete beach guard
- Stripe webhook idempotency + pending -> approved status update
- beach search `ILike`
- business reservations backend pagination + dashboard integration
- compose Redis cleanup
- `PROJECT_NOTES` current-state duzeltmesi
- smoke checklist ve ek regression testleri

Current state'te bir sonraki ajan once `git log --oneline -n 10` ve `ops/current-maintenance-analysis.md` uzerinden yeniden baglam kurmali.

## 0.1 2026-04-13 Ops Continuation Notes

Bugun yapilan ek dogrulamalar ve uygulanan degisiklikler:

- `cloudbuild.yaml` icindeki Artifact Registry project id typo'su duzeltildi.
- Cloud Run startup migration race'i icin PostgreSQL-backed integration test hatti eklendi; bos DB migration ve idempotency senaryolari `Testcontainers` ile dogrulandi.
- Revoked token cleanup BackgroundService yerine Cloud Scheduler + OIDC korumali internal endpoint'e tasindi ve canli tetikleme `200` ile dogrulandi.
- Monitoring tarafinda iki alert policy aktif:
  - Cloud Run 5xx ratio > %5
  - failed startup probe
- Cloud SQL `appuser` parolasi rotate edildi; Cloud Run artik plaintext `BEACHGO_DB_CONN` yerine Secret Manager `BEACHGO_DB_CONN` secret version `1` kullaniyor.
- Cloud Run uzerindeki retired revision'lar silindi; yalnizca `beachrehberi-api-00018-xsx` aktif ve metadata olarak kaldirildi.
- Runtime service account hardening uygulandi; `beach-api-sa` uzerinden `owner`, `storage.admin`, `artifactregistry.writer`, `cloudbuild.serviceAgent` ve project-level `secretmanager.secretAccessor` kaldirildi. Secret erisimi `BEACHGO_DB_CONN` ve `JWT_SECRET_KEY` uzerinde resource-level'e indirildi.
- Cloud Logging taramasinda plaintext DB parolasi veya tam connection string izi bulunmadi. Artifact Registry'de aktif `latest` digest disindaki eski image'lar silindi. Aktif Cloud Build log'unda bilinen secret pattern'i bulunmadi.
- Stripe live setup su an bilincli olarak devre disi birakildi. Cloud Run runtime env uzerinde `Features__UseRealPayment=false` explicit olarak tanimli.

Bugun itibariyla halen dis bagimlilik veya operator karari gerektiren blokajlar:

- `beachgo.net` mevcut hesapta `gcloud domains list-user-verified` altinda gorunmedigi icin `api.beachgo.net` custom domain mapping olusturulamadi.
- Stripe production setup ertelendi. Live `SecretKey` ve `WebhookSecret` henuz tanimli degil; flag bilincli olarak `false`.
- Git history secret cleanup konusunda `git filter-repo` / force-push karari alinmadi.
- Uygulamada ileride `Gcs:BucketName` aktif edilirse ilgili production bucket icin bucket-level object yazma izni ayri olarak verilmelidir; su an runtime bucket kullanmiyor.
- Docker daemon bu makinede o anda kapali oldugu icin aktif image layer icerigi binary seviyede acilip taranmadi; ancak registry eski digest cleanup'i tamamlandi.

## 1. DB Secret Rotation

- Durum: tamamlandi.
- Cloud SQL `appuser` parolasi yeni rastgele parola ile rotate edildi.
- Cloud Run `BEACHGO_DB_CONN` artik Secret Manager secret ref kullaniyor; plaintext env kaldirildi.
- Takip isi:
  - secret rotation takvimi tanimlayin
  - ileride GCS upload production'da aktif edilirse bucket-level IAM tanimlayin
  - Git history icin ayri secret scan/cleanup karari alin

## 2. Git History Secret Cleanup

- Eski commit'lerde gerçek secret bulunduysa `git filter-repo` veya `bfg-repo-cleaner` ile history temizliği yapılıp yapılmayacağına karar verin.
- History temizliği yapılmasa bile eski DB/JWT/SMTP/Stripe secret’larının tamamını rotate edin.
- Hassas config dosyalarının repo dışında tutulduğunu tekrar doğrulayın.

## 3. Stripe Production Setup

- Durum: ertelendi / bilincli olarak devre disi.
- Cloud Run runtime env: `Features__UseRealPayment=false`
- Canliya gecilecek zaman gerekli girdiler:
  - `Stripe__SecretKey=sk_live_...`
  - `Stripe__WebhookSecret=whsec_...`
  - `APP_URL=https://<frontend-public-origin>`
- Stripe live `SecretKey` ve `WebhookSecret` değerlerini alın.
- Canlı webhook endpoint URL’ini yapılandırın.
- `APP_URL`, success URL ve cancel URL’lerini canlı alan adına göre ayarlayın.
- Stripe dashboard üzerinden test ödeme ve webhook teslimini doğrulayın.
- Gerekli runtime config:
  - `Features__UseRealPayment=true`
  - `APP_URL=https://<frontend-public-origin>`
  - `Stripe__SecretKey=sk_live_...`
  - `Stripe__WebhookSecret=whsec_...`
- Beklenen webhook endpoint:
  - `https://<api-domain>/api/stripe/webhook`

## 4. Migration Apply Safety

- Migration uygulamadan önce canlı DB yedeği alın.
- Gerekirse SQL script üretip inceleyin.
- Migration’ları düşük trafik saatinde uygulayın.
- `ReservationPaymentStatusEnum` ve `AddRevokedTokenExpiresAt` migration’larının hedef ortamda başarıyla geçtiğini doğrulayın.
- Not:
  - Startup migration akisi su an uygulama boot'unda advisory lock ile serialize ediliyor.
  - Bu akis artik `Testcontainers` tabanli PostgreSQL integration test ile dogrulaniyor.
  - CI ortami icin Docker erisimi gerekecek; yoksa bu testler kosamaz.

## 5. Live Verification Checks

- OTP/e-posta doğrulama bypass edilemiyor mu kontrol edin.
- Rezervasyon fiyatı sadece server-side hesap mı kullanıyor doğrulayın.
- Loglarda plaintext OTP/JWT çıkmadığını kontrol edin.
- Logout sonrası aynı access token ile çağrının `401` döndüğünü test edin.
- Stripe checkout -> webhook -> `PaymentStatus=Paid` zincirini canlı ortamda uçtan uca test edin.
- Cloud Scheduler cleanup smoke:
  - `cleanup-revoked-tokens` job'i `europe-west1` bolgesinde `*/30 * * * *` ile calismali
  - Cloud Run request log'unda `Google-Cloud-Scheduler` user-agent ile `POST /internal/cleanup/revoked-tokens` icin `200` gorulmeli
