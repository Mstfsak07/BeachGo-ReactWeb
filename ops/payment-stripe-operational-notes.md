# Ödeme, Stripe ve HTTP semantikleri (operasyon / ürün)

Bu doküman, gerçek Stripe ödeme akışı ve webhook tamamlanana kadar API’nin davranışını netleştirir.

## Özellik bayrağı: `Features:UseRealPayment`

- **`false` (varsayılan):** Misafir rezervasyon **oluşturma** ve **ödeme** uçları bilinçli olarak devre dışıdır. İstemci **HTTP 503** ile `"Şu an ödeme sistemi devre dışı..."` benzeri mesaj alır. Bu, ürünün ödeme olmadan para toplamasını engeller.
- **`true`:** Rezervasyon oluşturma açılır; ancak sunucudaki `IPaymentService` (şu an `StripePaymentService`) gerçek tahsilatı tamamlayana kadar ödeme çağrısı **HTTP 501** ile `"Stripe ödeme entegrasyonu henüz hazır değil."` dönebilir.

Özet: **503** = ödeme özelliği kapalı; **501** = özellik açık ama sunucu tarafı entegrasyon henüz tamamlanmamış.

## Stripe webhook: `POST /api/stripe/webhook`

- Stripe Dashboard’dan gönderilen olaylar **yalnızca** `Stripe-Signature` başlığı ve **ham JSON gövde** ile `Stripe:WebhookSecret` kullanılarak doğrulanır (`EventUtility.ConstructEvent`).
- Secret yapılandırılmamış veya placeholder ise endpoint **503** döner; imza geçersizse **400**.
- Olay işleme (ör. `payment_intent.succeeded` sonrası rezervasyon güncelleme) entegrasyon tamamlandıkça bu uç üzerinden eklenecektir.

## Ortam değişkenleri (ASP.NET)

- `Stripe__WebhookSecret` — webhook imza doğrulaması için (appsettings `Stripe:WebhookSecret` ile aynı).
