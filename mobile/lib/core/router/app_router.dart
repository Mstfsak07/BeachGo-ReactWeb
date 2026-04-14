import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:beachgo/features/auth/presentation/providers/auth_provider.dart';
import 'package:beachgo/features/beach/presentation/screens/beach_detail_screen.dart';
import 'package:beachgo/features/beach/presentation/screens/beach_list_screen.dart';

// Sayfa importları — ilerleyen aşamalarda eklenir
// import '../features/auth/presentation/screens/login_screen.dart';
// import '../features/beach/presentation/screens/beach_list_screen.dart';
// ...

part 'app_router.g.dart';

// Route isimleri — magic string yerine enum kullan
enum AppRoute {
  home,
  beaches,
  beachDetail,
  events,
  login,
  register,
  businessRegister,
  profile,
  reservations,
  reservation,
  reservationSuccess,
  reservationCheck,
  favorites,
  forgotPassword,
  resetPassword,
  verifyEmail,
  dashboard,
  dashboardStats,
  dashboardReservations,
  beachSettings,
  admin,
  unauthorized,
}

extension AppRouteX on AppRoute {
  String get name => toString().split('.').last;
}

@riverpod
GoRouter router(Ref ref) {
  final authState = ref.watch(authProvider);

  return GoRouter(
    initialLocation: '/beaches',
    redirect: (context, state) {
      final isLoading = authState.isLoading;
      if (isLoading) return null; // Henüz bilinmiyor, bekle

      final isAuthenticated = authState.valueOrNull?.isAuthenticated ?? false;
      final isBusiness = authState.valueOrNull?.isBusiness ?? false;

      final path = state.matchedLocation;

      // Korumalı rotalar
      final protectedRoutes = ['/profile', '/reservations', '/favorites'];
      final businessRoutes = ['/dashboard', '/dashboard/stats',
          '/dashboard/reservations', '/beach-settings'];

      if (protectedRoutes.any((r) => path.startsWith(r)) && !isAuthenticated) {
        return '/login?redirect=${Uri.encodeComponent(path)}';
      }

      if (businessRoutes.any((r) => path.startsWith(r))) {
        if (!isAuthenticated) return '/login';
        if (!isBusiness) return '/unauthorized';
      }

      // Login/Register sayfalarına auth'lu kullanıcı giremez
      final guestOnlyRoutes = ['/login', '/register', '/business-register'];
      if (guestOnlyRoutes.contains(path) && isAuthenticated) {
        return isBusiness ? '/dashboard' : '/beaches';
      }

      return null;
    },
    routes: [
      GoRoute(
        path: '/',
        redirect: (_, __) => '/beaches',
      ),
      GoRoute(
        path: '/beaches',
        name: AppRoute.beaches.name,
        builder: (context, state) => const BeachListScreen(),
        routes: [
          GoRoute(
            path: ':id',
            name: AppRoute.beachDetail.name,
            builder: (context, state) {
              final id = int.tryParse(state.pathParameters['id'] ?? '');
              if (id == null) {
                return const _PlaceholderScreen('Gecersiz Plaj');
              }
              return BeachDetailScreen(beachId: id);
            },
          ),
        ],
      ),
      GoRoute(
        path: '/events',
        name: AppRoute.events.name,
        builder: (context, state) => const _PlaceholderScreen('Etkinlikler'),
      ),
      GoRoute(
        path: '/reservation/:beachId',
        name: AppRoute.reservation.name,
        builder: (context, state) {
          final beachId = state.pathParameters['beachId']!;
          return _PlaceholderScreen('Rezervasyon: $beachId');
        },
      ),
      GoRoute(
        path: '/reservation-success',
        name: AppRoute.reservationSuccess.name,
        builder: (context, state) => const _PlaceholderScreen('Rezervasyon Başarılı'),
      ),
      GoRoute(
        path: '/reservation-check',
        name: AppRoute.reservationCheck.name,
        builder: (context, state) => const _PlaceholderScreen('Rezervasyon Sorgula'),
      ),
      GoRoute(
        path: '/login',
        name: AppRoute.login.name,
        builder: (context, state) => const _PlaceholderScreen('Giriş'),
      ),
      GoRoute(
        path: '/register',
        name: AppRoute.register.name,
        builder: (context, state) => const _PlaceholderScreen('Kayıt'),
      ),
      GoRoute(
        path: '/business-register',
        name: AppRoute.businessRegister.name,
        builder: (context, state) => const _PlaceholderScreen('İşletme Kaydı'),
      ),
      GoRoute(
        path: '/forgot-password',
        name: AppRoute.forgotPassword.name,
        builder: (context, state) => const _PlaceholderScreen('Şifre Sıfırla'),
      ),
      GoRoute(
        path: '/profile',
        name: AppRoute.profile.name,
        builder: (context, state) => const _PlaceholderScreen('Profil'),
      ),
      GoRoute(
        path: '/reservations',
        name: AppRoute.reservations.name,
        builder: (context, state) => const _PlaceholderScreen('Rezervasyonlarım'),
      ),
      GoRoute(
        path: '/favorites',
        name: AppRoute.favorites.name,
        builder: (context, state) => const _PlaceholderScreen('Favorilerim'),
      ),
      GoRoute(
        path: '/dashboard',
        name: AppRoute.dashboard.name,
        builder: (context, state) => const _PlaceholderScreen('Dashboard'),
        routes: [
          GoRoute(
            path: 'stats',
            name: AppRoute.dashboardStats.name,
            builder: (context, state) => const _PlaceholderScreen('İstatistikler'),
          ),
          GoRoute(
            path: 'reservations',
            name: AppRoute.dashboardReservations.name,
            builder: (context, state) => const _PlaceholderScreen('Rezervasyonlar'),
          ),
        ],
      ),
      GoRoute(
        path: '/beach-settings',
        name: AppRoute.beachSettings.name,
        builder: (context, state) => const _PlaceholderScreen('Plaj Ayarları'),
      ),
      GoRoute(
        path: '/admin',
        name: AppRoute.admin.name,
        builder: (context, state) => const _PlaceholderScreen('Admin'),
      ),
      GoRoute(
        path: '/unauthorized',
        name: AppRoute.unauthorized.name,
        builder: (context, state) => const _PlaceholderScreen('Yetkisiz Erişim'),
      ),
    ],
  );
}

// Geçici placeholder — her sayfa implement edilirken değiştirilir
class _PlaceholderScreen extends StatelessWidget {
  final String title;
  const _PlaceholderScreen(this.title);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(title)),
      body: Center(
        child: Text(title, style: Theme.of(context).textTheme.headlineMedium),
      ),
    );
  }
}
