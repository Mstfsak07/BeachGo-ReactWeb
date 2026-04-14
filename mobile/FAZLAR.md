# BeachGo Flutter — Codex Geliştirme Fazları

Her faz bağımsız çalışır. Bir sonraki faza geçmeden önce mevcut fazı tamamla.
Her fazın sonunda `dart run build_runner build --delete-conflicting-outputs` çalıştır.

---

## BAŞLAMADAN ÖNCE — Mevcut Altyapı

Aşağıdaki dosyalar hazır, elle değiştirme:

```
lib/
├── main.dart
├── core/
│   ├── models/models.dart          ← Tüm Dart modelleri (freezed)
│   ├── network/
│   │   ├── app_config.dart         ← Base URL, timeout
│   │   ├── dio_client.dart         ← HTTP client + token interceptor
│   │   └── api_response.dart       ← unwrapResponse, friendlyError
│   ├── storage/storage_service.dart ← Secure token storage
│   ├── router/app_router.dart      ← Tüm route'lar + auth guard
│   └── theme/app_theme.dart        ← AppTheme, AppColors
├── features/auth/presentation/providers/auth_provider.dart
└── shared/widgets/shared_widgets.dart
```

**Build komutu (her fazdan sonra çalıştır):**
```bash
dart run build_runner build --delete-conflicting-outputs
```

---

## FAZ 1 — Kod Üretimi ve Auth Ekranları

**Amaç:** Build runner çalıştır, login ve register ekranlarını yaz.

### Görevler

**1.1 — Freezed ve Riverpod dosyalarını üret**

```bash
flutter pub get
dart run build_runner build --delete-conflicting-outputs
```

Bu komut şu dosyaları üretir:
- `lib/core/models/models.freezed.dart`
- `lib/core/models/models.g.dart`
- `lib/features/auth/presentation/providers/auth_provider.g.dart`
- `lib/core/router/app_router.g.dart`

**1.2 — Login ekranı**

Dosya: `lib/features/auth/presentation/screens/login_screen.dart`

Gereksinimler:
- Email ve password input alanları
- "Giriş Yap" butonu — `ref.read(authProvider.notifier).login(email, password)` çağırır
- Loading state'inde buton disabled + spinner
- Hata durumunda `ScaffoldMessenger` ile snackbar
- "Hesabın yok mu? Kayıt ol" → `/register` yönlendirir
- "Şifreni mi unuttun?" → `/forgot-password` yönlendirir
- AppColors ve AppTheme kullan

**1.3 — Register ekranı**

Dosya: `lib/features/auth/presentation/screens/register_screen.dart`

Gereksinimler:
- Ad soyad, email, password, password tekrar alanları
- Form validation (email format, password min 6 karakter, şifre eşleşmesi)
- `ref.read(authProvider.notifier).register(name, email, password)` çağırır
- Başarılı kayıt sonrası → `/login` yönlendir, "Lütfen emailinizi doğrulayın" mesajı göster

**1.4 — Router'daki placeholder'ları güncelle**

`lib/core/router/app_router.dart` dosyasında:
- `/login` route'unu `LoginScreen()` ile değiştir
- `/register` route'unu `RegisterScreen()` ile değiştir

### Bağımlılıklar
Bu faz başka hiçbir faza bağlı değil, direkt başlanabilir.

### Tamamlanma kriteri
- `flutter run` çalışır, uygulama açılır
- Login ekranı görünür, API'ye istek gider
- Başarılı girişte `/beaches` sayfasına yönlendirir (henüz placeholder)

---

## FAZ 2 — Beach Listesi

**Amaç:** Plaj listesi, arama ve filtreleme.

### Görevler

**2.1 — Beach repository**

Dosya: `lib/features/beach/data/repositories/beach_repository.dart`

```dart
// Web'deki services/api.ts karşılığı
// Şu metodları içerir:

Future<List<BeachDto>> getBeaches()
  // GET /Beaches

Future<BeachDto?> getBeachById(int id)
  // GET /Beaches/{id}

Future<List<BeachDto>> searchBeaches(String query)
  // GET /Beaches/search?q={query}

Future<List<BeachDto>> filterBeaches(BeachFilter filter)
  // POST /Beaches/filter — body: BeachFilter.toJson()

Future<List<EventDto>> getEvents()
  // GET /Events
```

Dio provider'ı kullan: `ref.read(dioProvider)`
`unwrapListResponse` ve `unwrapResponse` ile response parse et (api_response.dart'tan)

**2.2 — Beach provider**

Dosya: `lib/features/beach/presentation/providers/beach_provider.dart`

```dart
// Riverpod @riverpod ile:

@riverpod
Future<List<BeachDto>> beachList(Ref ref)
  // beach_repository.getBeaches() çağırır

@riverpod
Future<BeachDto> beachDetail(Ref ref, {required int id})
  // beach_repository.getBeachById(id) çağırır

// Arama için — debounce 400ms
@riverpod
class BeachSearch extends _$BeachSearch
  // query state tutar, searchBeaches çağırır

// Filtre için
@riverpod
class BeachFilterNotifier extends _$BeachFilterNotifier
  // BeachFilter state tutar, filterBeaches çağırır
```

**2.3 — BeachCard widget**

Dosya: `lib/features/beach/presentation/widgets/beach_card.dart`

Web'deki `BeachCard.tsx` referans al. İçerik:
- Plaj görseli (CachedNetworkImage, placeholder için BeachCardSkeleton)
- Plaj adı, lokasyon
- Rating yıldız göstergesi (★ 4.2)
- Doluluk yüzdesi
- Ücretsiz/ücretli badge
- Tıklanınca `/beaches/{id}` yönlendir

**2.4 — Beach list ekranı**

Dosya: `lib/features/beach/presentation/screens/beach_list_screen.dart`

- Üstte arama input'u
- Filtre chip'leri: Bar, Su Sporları, Çocuk Dostu, Havuz, Ücretsiz
- `beachListProvider` AsyncValue — loading'de 4 adet BeachCardSkeleton, error'da AppErrorWidget
- GridView (2 kolon) veya ListView ile BeachCard listesi
- Pull-to-refresh: `ref.refresh(beachListProvider)`

**2.5 — Router güncellemesi**

`/beaches` route'unu `BeachListScreen()` ile değiştir

### Bağımlılıklar
Faz 1 tamamlanmış olmalı (build_runner çalışmış olmalı).

### Tamamlanma kriteri
- Plajlar listelenir
- Arama çalışır (debounce'lu)
- Filtre chip'leri aktif/pasif toggle

---

## FAZ 3 — Beach Detay

**Amaç:** Plaj detay sayfası, hava durumu, yorumlar, favori.

### Görevler

**3.1 — Review ve Weather repository metodları**

`lib/features/beach/data/repositories/beach_repository.dart` dosyasına ekle:

```dart
Future<List<BeachReviewDto>> getBeachReviews(int beachId)
  // GET /Reviews/beach/{beachId}

Future<Map<String, dynamic>?> getBeachWeather(int beachId)
  // GET /Beaches/{beachId}/weather
  // Hata olursa null döndür, sessizce yut (web'deki gibi)

Future<void> createReview({required int beachId, required String userName,
    required double rating, required String comment})
  // POST /Reviews
```

**3.2 — Favorite repository**

Dosya: `lib/features/favorites/data/favorite_repository.dart`

```dart
Future<List<FavoriteDto>> getFavorites()
  // GET /users/favorites

Future<void> addFavorite(int beachId)
  // POST /users/favorites — body: { beachId }

Future<void> removeFavorite(int beachId)
  // DELETE /users/favorites/{beachId}
```

**3.3 — Favorite provider**

Dosya: `lib/features/favorites/presentation/providers/favorite_provider.dart`

```dart
@riverpod
class Favorites extends _$Favorites
  // getFavorites() ile başlar
  // toggle(beachId) metodu: varsa sil, yoksa ekle
  // Optimistic update: önce state'i güncelle, API başarısız olursa geri al

@riverpod
bool isFavorite(Ref ref, {required int beachId})
  // favoritesProvider'ı izler, beachId var mı kontrol eder
```

**3.4 — Beach detay ekranı**

Dosya: `lib/features/beach/presentation/screens/beach_detail_screen.dart`

Web'deki `BeachDetail.tsx` referans al. Bölümler:
1. Hero görsel + üstünde plaj adı, adres, rating
2. Favori butonu (kalp ikonu) — sadece giriş yapmışsa göster, `favoritesProvider`
3. Tesis chip'leri: Bar, Wi-Fi, Havuz vb. (varsa göster, yoksa gösterme)
4. Hava durumu kartı (weather null ise gösterme)
5. Rezervasyon CTA — tarih seçici + kişi sayısı + "Rezervasyon Yap" butonu → `/reservation/{id}`
6. Yorumlar listesi (`getBeachReviews`)
7. Yorum formu (giriş yapılmışsa göster)

**3.5 — Router güncellemesi**

`/beaches/:id` route'unu `BeachDetailScreen(id: id)` ile değiştir

### Bağımlılıklar
Faz 2 tamamlanmış olmalı.

### Tamamlanma kriteri
- Detay sayfası açılır, tüm bölümler yüklenir
- Favori toggle çalışır (giriş yapılmamışsa `/login`'e yönlendirir)
- Yorum gönderilebilir

---

## FAZ 4 — Rezervasyon Akışı (Guest)

**Amaç:** Web'deki GuestReservation wizard'ının Flutter karşılığı.

### Görevler

**4.1 — Reservation repository**

Dosya: `lib/features/reservation/data/repositories/reservation_repository.dart`

```dart
Future<ReservationDto?> createGuestReservation(Map<String, dynamic> data)
  // POST /GuestReservations

Future<String> sendOtp(String email)
  // POST /GuestReservations/send-otp — body: { email }
  // returns verificationId

Future<bool> verifyOtp(String verificationId, String code)
  // POST /GuestReservations/verify-otp — body: { verificationId, code }
  // returns verified bool

Future<ReservationDto?> getGuestReservation(String code, String email)
  // GET /GuestReservations/{code}?email={email}

Future<void> cancelGuestReservation(String code, String email)
  // POST /GuestReservations/cancel/{code} — body: { email }
```

**4.2 — Guest reservation form state**

Dosya: `lib/features/reservation/presentation/providers/guest_reservation_provider.dart`

Web'deki `GuestReservationFormData` karşılığı:

```dart
@freezed
class GuestReservationFormData with _$GuestReservationFormData {
  const factory GuestReservationFormData({
    @Default('') String reservationDate,
    @Default('10:00') String reservationTime,
    @Default('Standart') String reservationType,
    @Default(1) int personCount,
    @Default('') String note,
    @Default('') String firstName,
    @Default('') String lastName,
    @Default('') String phone,
    @Default('') String email,
    @Default('') String verificationId,
    @Default('') String otpCode,
    @Default(false) bool emailVerified,
    @Default(1) int currentStep,   // 1, 2, 3
  }) = _GuestReservationFormData;
}

@riverpod
class GuestReservation extends _$GuestReservation
  // state: GuestReservationFormData
  // updateField(...)
  // sendOtp() → step 2
  // verifyOtp() → step 3
  // createReservation(beachId) → ReservationDto
```

**4.3 — Rezervasyon ekranları**

Web'deki 3-adım wizard'ı:

`lib/features/reservation/presentation/screens/`

- `step1_personal_info.dart` — Ad, soyad, telefon, email alanları
- `step2_email_verify.dart` — OTP kodu girişi, "Tekrar Gönder" butonu
- `step3_success.dart` — Onay kodu, rezervasyon özeti
- `reservation_screen.dart` — Ana ekran, step indicator + adımlar arası geçiş

Step indicator widget: 3 daireli ilerleme göstergesi (web'deki StepIndicator gibi)

Yan panel (web'de sağda olan özet):
- Mobile'da `step1_personal_info`'nun üstünde "Rezervasyon Özeti" kartı olarak göster
- Plaj görseli, adı, seçilen tarih/kişi sayısı, tahmini ücret

**4.4 — Rezervasyon başarı ekranı**

Dosya: `lib/features/reservation/presentation/screens/reservation_success_screen.dart`

- Onay kodu (büyük, bold)
- Rezervasyon özeti
- "Rezervasyonumu Sorgula" → `/reservation-check`
- "Ana Sayfaya Dön" → `/beaches`

**4.5 — Router güncellemesi**

- `/reservation/:beachId` → `ReservationScreen`
- `/reservation-success` → `ReservationSuccessScreen`

### Bağımlılıklar
Faz 3 tamamlanmış olmalı. Auth opsiyonel (misafir rezervasyonu).

### Tamamlanma kriteri
- 3 adım sırasıyla çalışır
- OTP gönderilir ve doğrulanır
- Rezervasyon oluşturulur, başarı ekranı açılır

---

## FAZ 5 — Profil, Rezervasyonlarım, Favorilerim

**Amaç:** Giriş yapılmış kullanıcı ekranları.

### Görevler

**5.1 — User repository**

Dosya: `lib/features/profile/data/user_repository.dart`

```dart
Future<AppUser> getProfile()
  // GET /users/profile

Future<AppUser> updateProfile(Map<String, dynamic> data)
  // PUT /users/profile

Future<void> changePassword(String current, String newPassword)
  // PUT /users/change-password
```

**5.2 — Profil ekranı**

Dosya: `lib/features/profile/presentation/screens/profile_screen.dart`

- Kullanıcı adı, email gösterimi
- Profil düzenleme formu (ad, soyad, telefon)
- Şifre değiştirme bölümü (ayrı bir tile)
- Çıkış yap butonu → `showConfirmDialog` + `ref.read(authProvider.notifier).logout()`

**5.3 — Rezervasyonlarım ekranı**

Dosya: `lib/features/reservation/presentation/screens/my_reservations_screen.dart`

```dart
// Repository'ye ekle:
Future<List<ReservationDto>> getUserReservations()
  // GET /Reservations/my
```

- Rezervasyon kartları: plaj adı, tarih, kişi sayısı, durum badge'i
- Durum renkleri: Approved=yeşil, Pending=sarı, Cancelled/Rejected=kırmızı
- İptal butonu (sadece Pending olanlar) → `showConfirmDialog` + iptal API çağrısı

**5.4 — Favorilerim ekranı**

Dosya: `lib/features/favorites/presentation/screens/favorites_screen.dart`

- `favoritesProvider` izle
- BeachCard grid'i (Faz 2'deki widget'ı kullan)
- Favori yoksa boş durum mesajı: "Henüz favori eklemediniz"

**5.5 — Router güncellemesi**

- `/profile` → `ProfileScreen`
- `/reservations` → `MyReservationsScreen`
- `/favorites` → `FavoritesScreen`

### Bağımlılıklar
Faz 3 tamamlanmış olmalı.

### Tamamlanma kriteri
- Profil düzenlenebilir
- Rezervasyonlar listelenir ve iptal edilebilir
- Favoriler gösterilir

---

## FAZ 6 — Reservation Check + Auth Utilities

**Amaç:** Misafir rezervasyon sorgulama, şifre sıfırlama, email doğrulama.

### Görevler

**6.1 — Reservation check ekranı**

Dosya: `lib/features/reservation/presentation/screens/reservation_check_screen.dart`

Web'deki `ReservationCheck.tsx` referans al:
- Onay kodu + email ile sorgulama formu
- Sonuç: rezervasyon detay kartı
- İptal butonu → `showConfirmDialog` (danger=true)

**6.2 — Forgot password ekranı**

Dosya: `lib/features/auth/presentation/screens/forgot_password_screen.dart`

```dart
// AuthService'e ekle veya auth_provider'da metod aç:
Future<void> forgotPassword(String email)
  // POST /Auth/forgot-password
```

- Email input
- "Link Gönder" butonu
- Başarıda: "Emailinize link gönderdik" mesajı

**6.3 — Reset password ekranı**

Dosya: `lib/features/auth/presentation/screens/reset_password_screen.dart`

- URL'den `token` query parametresini al: `state.uri.queryParameters['token']`
- Yeni şifre + tekrar alanları
- POST /Auth/reset-password

**6.4 — Verify email ekranı**

Dosya: `lib/features/auth/presentation/screens/verify_email_screen.dart`

- URL'den `token` query parametresini al
- Sayfa açılınca GET /Auth/verify-email?token={token} çağır
- Başarı / hata durumunu göster

**6.5 — Router güncellemesi**

- `/reservation-check` → `ReservationCheckScreen`
- `/forgot-password` → `ForgotPasswordScreen`
- `/reset-password` → `ResetPasswordScreen`
- `/verify-email` → `VerifyEmailScreen`

### Bağımlılıklar
Faz 1 tamamlanmış olmalı.

### Tamamlanma kriteri
- Rezervasyon onay koduyla sorgulanabilir ve iptal edilebilir
- Şifre sıfırlama akışı çalışır

---

## FAZ 7 — Bottom Navigation + App Shell

**Amaç:** Ana navigasyon yapısını kur.

### Görevler

**7.1 — App shell**

Dosya: `lib/core/router/app_shell.dart`

`StatefulShellRoute` (go_router) kullanarak bottom navigation kur.

Tab'lar:
- Plajlar (beach_list_screen) — ikonu: `Icons.beach_access`
- Etkinlikler (events_screen) — ikonu: `Icons.event`
- Rezervasyonlarım (my_reservations_screen — giriş gerekmez, yoksa login yönlendir)
- Profil (profile_screen — giriş gerekmez, yoksa login yönlendir)

**7.2 — Events ekranı**

Dosya: `lib/features/beach/presentation/screens/events_screen.dart`

```dart
// Repository'ye ekle:
Future<List<EventDto>> getEvents()
  // GET /Events
```

- Event kartları: başlık, tarih, plaj adı, görsel
- Basit ListView, AsyncValue ile loading/error state

**7.3 — Router'ı shell ile yeniden yapılandır**

`app_router.dart`'ı `StatefulShellRoute` kullanacak şekilde güncelle.
Shell dışındaki route'lar (rezervasyon wizard, detay vb.) overlay olarak açılır.

### Bağımlılıklar
Faz 2-5 tamamlanmış olmalı.

### Tamamlanma kriteri
- Bottom navigation çalışır
- Tab geçişlerinde state korunur
- Shell dışı sayfalar tam ekran açılır

---

## FAZ 8 — Business Dashboard (Opsiyonel)

**Amaç:** İşletme hesapları için dashboard.

> Bu faz düşük öncelikli — kullanıcı odaklı fazlar tamamlandıktan sonra yap.

### Görevler

**8.1 — Business repository**

Dosya: `lib/features/dashboard/data/business_repository.dart`

```dart
Future<List<ReservationDto>> getBusinessReservations()
  // GET /business/reservations

Future<Map<String, dynamic>> getBusinessStats()
  // GET /business/stats

Future<void> approveReservation(int id)
  // PUT /business/reservations/{id}/approve

Future<void> rejectReservation(int id)
  // PUT /business/reservations/{id}/reject
```

**8.2 — Dashboard ana ekran**

Dosya: `lib/features/dashboard/presentation/screens/dashboard_screen.dart`

- İstatistik kartları: toplam rezervasyon, bugünkü girişler, tahmini kazanç
- Son rezervasyonlar listesi
- Her rezervasyonda Onayla / Reddet butonları

**8.3 — Beach settings ekranı**

Dosya: `lib/features/dashboard/presentation/screens/beach_settings_screen.dart`

```dart
// Repository'ye ekle:
Future<BeachDto?> getMyBeach()    // GET /business/beach
Future<void> updateMyBeach(Map<String, dynamic> data)  // PUT /business/beach
```

- Plaj bilgileri formu (ad, adres, açıklama, kapasite, saat)
- Tesis toggle'ları (Bar, Wi-Fi, Havuz vb.)
- Kaydet butonu

**8.4 — Dashboard navigasyonu**

Business rolündeki kullanıcılar için ayrı bottom nav veya drawer:
- Dashboard ana
- Rezervasyonlar
- Plaj Ayarları

### Bağımlılıklar
Faz 7 tamamlanmış olmalı.

---

## Genel Kurallar (Her Fazda Geçerli)

1. Her dosyanın başına web karşılığını yorum olarak yaz:
   `// Web: src/pages/BeachDetail.tsx`

2. API hataları her zaman `friendlyError(e)` ile kullanıcıya göster:
   ```dart
   } catch (e) {
     ScaffoldMessenger.of(context).showSnackBar(
       SnackBar(content: Text(friendlyError(e))),
     );
   }
   ```

3. Loading state'lerinde skeleton veya `AppLoadingIndicator` kullan, boş `Container` koyma.

4. Rezervasyon iptali gibi destructive işlemlerde her zaman `showConfirmDialog` çağır.

5. Giriş gerektiren işlemlerde `isAuthenticated` false ise
   `context.push('/login')` yönlendir, işlemi yapma.

6. Her fazdan sonra mutlaka build_runner çalıştır.
