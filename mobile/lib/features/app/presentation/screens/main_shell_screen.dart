import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:beachgo/core/router/app_router.dart';
import 'package:beachgo/core/theme/app_theme.dart';
import 'package:beachgo/features/auth/presentation/providers/auth_provider.dart';
import 'package:beachgo/features/beach/presentation/screens/beach_list_screen.dart';
import 'package:beachgo/features/favorites/presentation/screens/favorite_beaches_screen.dart';
import 'package:beachgo/features/reservation/presentation/screens/reservation_list_screen.dart';

enum AppShellTab {
  home('home', 'Kesfet', Icons.explore_outlined, Icons.explore_rounded),
  favorites('favorites', 'Favoriler', Icons.favorite_border_rounded,
      Icons.favorite_rounded),
  reservations('reservations', 'Rezervasyonlar', Icons.event_note_outlined,
      Icons.event_note_rounded),
  profile('profile', 'Profil', Icons.person_outline_rounded, Icons.person_rounded);

  const AppShellTab(this.pathSegment, this.label, this.icon, this.selectedIcon);

  final String pathSegment;
  final String label;
  final IconData icon;
  final IconData selectedIcon;

  static AppShellTab fromSegment(String segment) {
    return AppShellTab.values.firstWhere(
      (tab) => tab.pathSegment == segment,
      orElse: () => AppShellTab.home,
    );
  }
}

class MainShellScreen extends ConsumerWidget {
  const MainShellScreen({
    super.key,
    required this.currentTab,
  });

  final AppShellTab currentTab;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Scaffold(
      body: _buildTabBody(ref),
      bottomNavigationBar: SafeArea(
        top: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(12, 0, 12, 12),
          child: Container(
            decoration: BoxDecoration(
              color: AppColors.surface,
              borderRadius: BorderRadius.circular(28),
              border: Border.all(color: AppColors.slate100),
            ),
            child: ClipRRect(
              borderRadius: BorderRadius.circular(28),
              child: NavigationBar(
                selectedIndex: currentTab.index,
                onDestinationSelected: (index) {
                  final tab = AppShellTab.values[index];
                  if (tab == currentTab) {
                    return;
                  }
                  context.goNamed(
                    AppRoute.appShell.name,
                    pathParameters: {'tab': tab.pathSegment},
                  );
                },
                destinations: [
                  for (final tab in AppShellTab.values)
                    NavigationDestination(
                      icon: Icon(tab.icon),
                      selectedIcon: Icon(tab.selectedIcon),
                      label: tab.label,
                    ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildTabBody(WidgetRef ref) {
    final isAuthenticated = ref.watch(isAuthenticatedProvider);
    final user = ref.watch(currentUserProvider);

    switch (currentTab) {
      case AppShellTab.home:
        return const BeachListScreen();
      case AppShellTab.favorites:
        return isAuthenticated
            ? const FavoriteBeachesScreen()
            : const _GuestProtectedScreen(
                title: 'Favoriler',
                description:
                    'Favori plajlari kaydetmek icin giris yapmaniz gerekiyor.',
                icon: Icons.favorite_rounded,
              );
      case AppShellTab.reservations:
        return isAuthenticated
            ? const ReservationListScreen()
            : const _GuestProtectedScreen(
                title: 'Rezervasyonlar',
                description:
                    'Rezervasyon gecmisinizi gormek icin giris yapmaniz gerekiyor.',
                icon: Icons.event_note_rounded,
              );
      case AppShellTab.profile:
        return isAuthenticated
            ? _ProfilePlaceholderScreen(userName: user?.displayName ?? 'Kullanici')
            : const _GuestProtectedScreen(
                title: 'Profil',
                description:
                    'Profil ve hesap bilgilerinize ulasmak icin giris yapin.',
                icon: Icons.person_rounded,
              );
    }
  }
}

class _GuestProtectedScreen extends StatelessWidget {
  const _GuestProtectedScreen({
    required this.title,
    required this.description,
    required this.icon,
  });

  final String title;
  final String description;
  final IconData icon;

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      child: Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 420),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Container(
                  width: 84,
                  height: 84,
                  decoration: BoxDecoration(
                    color: AppColors.primaryLight,
                    borderRadius: BorderRadius.circular(28),
                    border: Border.all(color: AppColors.panelBorder),
                  ),
                  child: Icon(icon, color: AppColors.primary, size: 40),
                ),
                const SizedBox(height: 20),
                Text(
                  title,
                  style: Theme.of(context).textTheme.headlineMedium,
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 10),
                Text(
                  description,
                  textAlign: TextAlign.center,
                  style: Theme.of(context).textTheme.bodyLarge,
                ),
                const SizedBox(height: 20),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: () => context.goNamed(AppRoute.login.name),
                    child: const Text('Giris Yap'),
                  ),
                ),
                const SizedBox(height: 10),
                SizedBox(
                  width: double.infinity,
                  child: TextButton(
                    onPressed: () => context.goNamed(
                      AppRoute.appShell.name,
                      pathParameters: {'tab': AppShellTab.home.pathSegment},
                    ),
                    child: const Text('Kesfetmeye Don'),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _ProfilePlaceholderScreen extends ConsumerWidget {
  const _ProfilePlaceholderScreen({required this.userName});

  final String userName;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return SafeArea(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Profil', style: Theme.of(context).textTheme.headlineMedium),
            const SizedBox(height: 8),
            Text(
              'Merhaba, $userName',
              style: Theme.of(context).textTheme.bodyLarge,
            ),
            const SizedBox(height: 24),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(18),
              decoration: BoxDecoration(
                color: AppColors.surfaceRaised,
                borderRadius: BorderRadius.circular(18),
                border: Border.all(color: AppColors.panelBorder, width: 0.8),
              ),
              child: Text(
                'Profil detaylari ve hesap ayarlari bu alanda genisletilecek. Simdilik guvenli sekilde cikis yapabilirsiniz.',
                style: Theme.of(context).textTheme.bodyLarge,
              ),
            ),
            const SizedBox(height: 20),
            SizedBox(
              width: double.infinity,
              child: OutlinedButton(
                onPressed: () async {
                  await ref.read(authProvider.notifier).logout();
                  if (context.mounted) {
                    context.goNamed(AppRoute.login.name);
                  }
                },
                child: const Text('Cikis Yap'),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
