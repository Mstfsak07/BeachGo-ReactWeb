# Remaining Operations Runbook

## 1. DB Secret Rotation

- `appsettings.json` içindeki geçmişte kullanılan varsayılan DB parolasını geçersiz kılın.
- Yeni DB parolasını yalnızca environment variable veya secret manager üzerinden verin.
- Uygulama DB kullanıcısının yetkilerini en aza indirin.

## 2. Git History Secret Cleanup

- Eski commit'lerde gerçek secret bulunduysa `git filter-repo` veya `bfg-repo-cleaner` ile history temizliği yapılıp yapılmayacağına karar verin.
- History temizliği yapılmasa bile eski DB/JWT/SMTP/Stripe secret’larının tamamını rotate edin.
- Hassas config dosyalarının repo dışında tutulduğunu tekrar doğrulayın.

## 3. Stripe Production Setup

- Stripe live `SecretKey` ve `WebhookSecret` değerlerini alın.
- Canlı webhook endpoint URL’ini yapılandırın.
- `APP_URL`, success URL ve cancel URL’lerini canlı alan adına göre ayarlayın.
- Stripe dashboard üzerinden test ödeme ve webhook teslimini doğrulayın.

## 4. Migration Apply Safety

- Migration uygulamadan önce canlı DB yedeği alın.
- Gerekirse SQL script üretip inceleyin.
- Migration’ları düşük trafik saatinde uygulayın.
- `ReservationPaymentStatusEnum` ve `AddRevokedTokenExpiresAt` migration’larının hedef ortamda başarıyla geçtiğini doğrulayın.

## 5. Live Verification Checks

- OTP/e-posta doğrulama bypass edilemiyor mu kontrol edin.
- Rezervasyon fiyatı sadece server-side hesap mı kullanıyor doğrulayın.
- Loglarda plaintext OTP/JWT çıkmadığını kontrol edin.
- Logout sonrası aynı access token ile çağrının `401` döndüğünü test edin.
- Stripe checkout -> webhook -> `PaymentStatus=Paid` zincirini canlı ortamda uçtan uca test edin.
