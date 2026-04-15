import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:beachgo/features/app/presentation/screens/main_shell_screen.dart';
import 'package:beachgo/features/auth/presentation/providers/auth_provider.dart';
import 'package:beachgo/features/auth/presentation/screens/login_screen.dart';
import 'package:beachgo/features/auth/presentation/screens/register_screen.dart';
import 'package:beachgo/features/beach/domain/entities/beach.dart';
import 'package:beachgo/features/beach/presentation/screens/beach_detail_screen.dart';
import 'package:beachgo/features/beach/presentation/screens/beach_list_screen.dart';
import 'package:beachgo/features/favorites/presentation/screens/favorite_beaches_screen.dart';
import 'package:beachgo/features/reservation/domain/entities/reservation_confirmation.dart';
import 'package:beachgo/features/reservation/presentation/screens/reservation_list_screen.dart';
import 'package:beachgo/features/reservation/presentation/screens/reservation_screen.dart';
import 'package:beachgo/features/reservation/presentation/screens/reservation_success_screen.dart';
import 'package:beachgo/features/stories/presentation/screens/story_admin_screen.dart';

// Sayfa importları — ilerleyen aşamalarda eklenir
// import '../features/auth/presentation/screens/login_screen.dart';
// import '../features/beach/presentation/screens/beach_list_screen.dart';
// ...

part 'app_router.g.dart';

// Route isimleri — magic string yerine enum kullan
enum AppRoute {
  startup,
  appShell,
  home,
  beaches,
  beachDetail,
  events,
  login,
  businessLogin,
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
    initialLocation: '/',
    redirect: (context, state) {
      final isBootstrapping =
          authState.isLoading &&
          !authState.hasValue &&
          !authState.hasError;
      final path = state.matchedLocation;
      final redirectTarget = state.uri.queryParameters['redirect'];

      if (isBootstrapping) {
        return path == '/' ? null : '/';
      }

      final isAuthenticated = authState.valueOrNull?.isAuthenticated ?? false;
      final isBusiness = authState.valueOrNull?.isBusiness ?? false;

      if (path == '/') {
        return '/app/home';
      }

      // Korumalı rotalar
      final protectedRoutes = [
        '/profile',
        '/reservations',
        '/favorites',
        '/reservation',
      ];
      final businessRoutes = ['/dashboard', '/dashboard/stats',
          '/dashboard/reservations', '/beach-settings', '/admin'];

      if (protectedRoutes.any((r) => path.startsWith(r)) && !isAuthenticated) {
        return '/login?redirect=${Uri.encodeComponent(path)}';
      }

      if (businessRoutes.any((r) => path.startsWith(r))) {
        if (!isAuthenticated) return '/login';
        if (!isBusiness) return '/unauthorized';
      }

      // Login/Register sayfalarına auth'lu kullanıcı giremez
      final guestOnlyRoutes = [
        '/login',
        '/business-login',
        '/register',
        '/business-register',
      ];
      if (guestOnlyRoutes.contains(path) && isAuthenticated) {
        if (redirectTarget != null && redirectTarget.isNotEmpty) {
          return redirectTarget;
        }
        return isBusiness ? '/dashboard' : '/app/home';
      }

      return null;
    },
    routes: [
      GoRoute(
        path: '/',
        name: AppRoute.startup.name,
        builder: (context, state) => const _StartupScreen(),
      ),
      GoRoute(
        path: '/app/:tab',
        name: AppRoute.appShell.name,
        builder: (context, state) {
          final tab = AppShellTab.fromSegment(
            state.pathParameters['tab'] ?? AppShellTab.home.pathSegment,
          );
          return MainShellScreen(currentTab: tab);
        },
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
          final beachId = int.tryParse(state.pathParameters['beachId'] ?? '');
          if (beachId == null) {
            return const _PlaceholderScreen('Gecersiz Rezervasyon');
          }
          return ReservationScreen(
            beachId: beachId,
            initialBeach: state.extra is Beach ? state.extra as Beach : null,
          );
        },
      ),
      GoRoute(
        path: '/reservation-success',
        name: AppRoute.reservationSuccess.name,
        builder: (context, state) {
          final confirmation = state.extra;
          if (confirmation is! ReservationConfirmation) {
            return const _PlaceholderScreen('Rezervasyon Basarili');
          }
          return ReservationSuccessScreen(confirmation: confirmation);
        },
      ),
      GoRoute(
        path: '/reservation-check',
        name: AppRoute.reservationCheck.name,
        builder: (context, state) => const _PlaceholderScreen('Rezervasyon Sorgula'),
      ),
      GoRoute(
        path: '/login',
        name: AppRoute.login.name,
        builder: (context, state) => const LoginScreen(),
      ),
      GoRoute(
        path: '/business-login',
        name: AppRoute.businessLogin.name,
        builder: (context, state) => const LoginScreen(isBusinessMode: true),
      ),
      GoRoute(
        path: '/register',
        name: AppRoute.register.name,
        builder: (context, state) => const RegisterScreen(),
      ),
      GoRoute(
        path: '/business-register',
        name: AppRoute.businessRegister.name,
        builder: (context, state) => const RegisterScreen(isBusiness: true),
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
        builder: (context, state) => const ReservationListScreen(),
      ),
      GoRoute(
        path: '/favorites',
        name: AppRoute.favorites.name,
        builder: (context, state) => const FavoriteBeachesScreen(),
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
        builder: (context, state) => const StoryAdminScreen(),
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

class _StartupScreen extends StatelessWidget {
  const _StartupScreen();

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(
        child: CircularProgressIndicator(),
      ),
    );
  }
}
