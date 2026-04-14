# BeachGo Flutter

BeachGo web uygulamasının Flutter mobile versiyonu.

## Teknoloji Stack

| Katman | Paket |
|--------|-------|
| State management | flutter_riverpod + riverpod_annotation |
| Navigation | go_router |
| HTTP | dio |
| Storage | flutter_secure_storage (token) + shared_preferences (tercihler) |
| Model | freezed + json_serializable |

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
│   ├── models/        # Tüm Dart modelleri (web types.ts karşılığı)
│   ├── network/       # Dio client, interceptor, response wrapper
│   ├── storage/       # Secure storage abstraction
│   ├── router/        # go_router + auth guard
│   └── theme/         # AppTheme, AppColors
│
├── features/
│   ├── auth/
│   │   ├── data/
│   │   └── presentation/
│   │       ├── providers/   # auth_provider.dart
│   │       ├── screens/     # LoginScreen, RegisterScreen...
│   │       └── widgets/
│   ├── beach/
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
| `types.ts` | `core/models/models.dart` |
| `context/AuthContext.tsx` | `features/auth/presentation/providers/auth_provider.dart` |
| `lib/storage.ts` | `core/storage/storage_service.dart` |
| `App.js` (routing) | `core/router/app_router.dart` |
| `window.confirm` | `shared/widgets/shared_widgets.dart → showConfirmDialog()` |
| Tailwind renkler | `core/theme/app_theme.dart → AppColors` |

## Kod Üretimi (build_runner)

`freezed` ve `riverpod_annotation` kullanıldığı için `.g.dart` ve `.freezed.dart`
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
