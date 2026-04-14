# BeachGo Flutter

BeachGo web uygulamasının Flutter mobile versiyonu.

## Teknoloji Stack

| Katman | Paket |
|--------|-------|
| State management | flutter_riverpod + riverpod_annotation |
| Navigation | go_router |
| HTTP | dio |
| Storage | flutter_secure_storage (token) + shared_preferences (tercihler) |
| Model | DTO + domain entity mapping |

## Kurulum

```bash
flutter pub get
dart run build_runner build --delete-conflicting-outputs
```

## Çalıştırma

```bash
# Development (Android emülatör — localhost için 10.0.2.2)
flutter run

# Development (iOS simülatör — app_config.dart'ta localhost'u aç)
flutter run

# Production
flutter run --dart-define=ENV=production
```

## Proje Yapısı

```
lib/
├── core/
│   ├── error/         # Failure ve Result tipleri
│   ├── network/       # Dio client, interceptor, response wrapper
│   ├── storage/       # Secure storage abstraction
│   ├── router/        # go_router + auth guard
│   └── theme/         # AppTheme, AppColors
│
├── features/
│   ├── auth/
│   │   ├── data/
│   │   │   └── models/      # DTO modelleri
│   │   ├── domain/
│   │   │   └── entities/    # Domain entity'leri
│   │   └── presentation/
│   │       ├── providers/   # auth_provider.dart
│   │       ├── screens/     # LoginScreen, RegisterScreen...
│   │       └── widgets/
│   ├── beach/
│   │   ├── data/
│   │   │   ├── models/      # DTO modelleri
│   │   │   └── repository/
│   │   ├── domain/
│   │   │   └── entities/    # Domain entity'leri
│   │   └── presentation/
│   ├── reservation/
│   ├── profile/
│   └── favorites/
│
└── shared/
    └── widgets/       # AppLoadingIndicator, showConfirmDialog, Skeleton...
```

## Web → Flutter Eşleşmeleri

| Web | Flutter |
|-----|---------|
| `api/axios.ts` | `core/network/dio_client.dart` |
| `api/token.ts` | `core/storage/storage_service.dart` |
| `types.ts` | `features/*/data/models/*_dto.dart` + `features/*/domain/entities/*.dart` |
| `context/AuthContext.tsx` | `features/auth/presentation/providers/auth_provider.dart` |
| `lib/storage.ts` | `core/storage/storage_service.dart` |
| `App.js` (routing) | `core/router/app_router.dart` |
| `window.confirm` | `shared/widgets/shared_widgets.dart → showConfirmDialog()` |
| Tailwind renkler | `core/theme/app_theme.dart → AppColors` |

## Kod Üretimi (build_runner)

`riverpod_annotation` ve router generation kullanıldığı için `.g.dart`
dosyaları elle yazılmaz, üretilir:

```bash
# Bir kez çalıştır
dart run build_runner build --delete-conflicting-outputs

# Geliştirme sırasında watch modunda
dart run build_runner watch --delete-conflicting-outputs
```

## Sayfa Geliştirme Sırası

- [ ] Auth (Login, Register)
- [ ] Beach List
- [ ] Beach Detail
- [ ] Guest Reservation flow
- [ ] My Reservations
- [ ] Profile
- [ ] Favorites
- [ ] Business Dashboard
