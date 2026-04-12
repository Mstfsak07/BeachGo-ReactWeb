# Smoke Checklist

Bu liste current-state icin hizli manuel dogrulama amaclidir. UI akislari ile kritik backend entegrasyonlarini kapsar.

## Auth

- [ ] Gecerli business veya admin kullanicisi login oldugunda uygulama aciliyor ve korumali ekranlar erisilebilir oluyor.
- [ ] Hatali e-posta veya sifre denemesi basarili login uretmiyor ve uygun hata mesaji donuyor.
- [ ] Sayfa yenilemeden sonra auth durumu korunuyor; uygulama refresh cookie uzerinden access token yenileyebiliyor.
- [ ] Logout sonrasi korumali sayfalara geri donus login'e yonlendiriliyor.
- [ ] Ayni IP'den auth endpointlerine hizli tekrar istek atildiginda rate limit devreye girebiliyor.

## Guest Reservation

- [ ] Guest OTP gonderimi basarili calisiyor ve verification id uretiliyor.
- [ ] Dogru OTP ile verify sonrasi guest reservation olusturulabiliyor.
- [ ] Verify edilmemis veya gecersiz verification id ile reservation create reddediliyor.
- [ ] Soft-deleted beach icin guest reservation create kabul edilmiyor.
- [ ] Guest reservation fiyatı backend tarafinda hesaplanıyor; istemciden gelen tutara guvenilmiyor.
- [ ] Payment start adiminda guest reservation icin Stripe checkout oturumu veya beklenen payment response donuyor.

## Stripe Webhook

- [ ] `checkout.session.completed` webhook'u gecerli imza ile kabul ediliyor.
- [ ] Basarili webhook sonrasi ilgili reservation `PaymentStatus=Paid` oluyor.
- [ ] Basarili webhook sonrasi `Pending` reservation durumu `Approved` oluyor.
- [ ] Ayni completed event tekrar geldiginde ikinci kez payment kaydi veya durum mutasyonu uretilmiyor.

## Business Dashboard

- [ ] Business reservations ekrani rezervasyon listesini yukleyebiliyor.
- [ ] Business reservations endpoint'i `page` ve `pageSize` ile sinirli liste donduruyor.
- [ ] Business stats ekrani toplam rezervasyon, gunluk check-in ve estimated earnings alanlarini hata vermeden gosteriyor.
