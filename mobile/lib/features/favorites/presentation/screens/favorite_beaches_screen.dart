import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:gap/gap.dart';
import 'package:go_router/go_router.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/theme/app_theme.dart';
import 'package:beachgo/features/favorites/presentation/providers/favorite_beaches_provider.dart';
import 'package:beachgo/shared/widgets/shared_widgets.dart';

class FavoriteBeachesScreen extends ConsumerWidget {
  const FavoriteBeachesScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final favoritesAsync = ref.watch(favoriteBeachesProvider);

    return SafeArea(
      child: RefreshIndicator(
        onRefresh: () => ref.read(favoriteBeachesProvider.notifier).refresh(),
        child: favoritesAsync.when(
          data: (favorites) => favorites.isEmpty
              ? const _FavoritesEmpty()
              : ListView.separated(
                  padding: const EdgeInsets.fromLTRB(20, 24, 20, 24),
                  itemCount: favorites.length,
                  separatorBuilder: (_, __) => const Gap(14),
                  itemBuilder: (context, index) {
                    final beach = favorites[index];
                    return InkWell(
                      onTap: () => context.push('/beaches/${beach.id}'),
                      borderRadius: BorderRadius.circular(22),
                      child: Ink(
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: AppColors.surfaceRaised,
                          borderRadius: BorderRadius.circular(22),
                          border: Border.all(color: AppColors.panelBorder),
                        ),
                        child: Row(
                          children: [
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    beach.name,
                                    style: Theme.of(context)
                                        .textTheme
                                        .titleLarge,
                                  ),
                                  const Gap(6),
                                  Text(
                                    beach.address.isNotEmpty
                                        ? beach.address
                                        : 'Konum bilgisi hazir degil',
                                    style:
                                        Theme.of(context).textTheme.bodyMedium,
                                  ),
                                  const Gap(10),
                                  Wrap(
                                    spacing: 8,
                                    runSpacing: 8,
                                    children: [
                                      _MetaPill(
                                        icon: Icons.star_rounded,
                                        label: beach.rating > 0
                                            ? beach.rating.toStringAsFixed(1)
                                            : 'Yeni',
                                      ),
                                      _MetaPill(
                                        icon: Icons.people_alt_outlined,
                                        label:
                                            '%${beach.occupancyPercent.toStringAsFixed(0)} dolu',
                                      ),
                                    ],
                                  ),
                                ],
                              ),
                            ),
                            IconButton(
                              onPressed: () async {
                                final failure = await ref
                                    .read(favoriteBeachesProvider.notifier)
                                    .toggleFavorite(beach);
                                if (failure != null && context.mounted) {
                                  ScaffoldMessenger.of(context).showSnackBar(
                                    SnackBar(content: Text(failure.message)),
                                  );
                                }
                              },
                              icon: const Icon(
                                Icons.favorite_rounded,
                                color: AppColors.error,
                              ),
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
          loading: () => const Center(child: AppLoadingIndicator()),
          error: (error, _) => AppErrorWidget(
            message: error is Failure && error.message.isNotEmpty
                ? error.message
                : 'Favoriler yuklenemedi.',
            onRetry: () => ref.read(favoriteBeachesProvider.notifier).refresh(),
          ),
        ),
      ),
    );
  }
}

class _FavoritesEmpty extends StatelessWidget {
  const _FavoritesEmpty();

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.fromLTRB(24, 80, 24, 24),
      children: [
        Container(
          width: 84,
          height: 84,
          margin: const EdgeInsets.only(bottom: 20),
          decoration: BoxDecoration(
            color: AppColors.primaryLight,
            borderRadius: BorderRadius.circular(28),
          ),
          child: const Icon(
            Icons.favorite_border_rounded,
            color: AppColors.primary,
            size: 38,
          ),
        ),
        Text('Favoriler', style: Theme.of(context).textTheme.headlineMedium),
        const Gap(10),
        Text(
          'Begendiginiz plajlari favorilere eklediginizde burada hizlica ulasabilirsiniz.',
          style: Theme.of(context).textTheme.bodyLarge,
        ),
      ],
    );
  }
}

class _MetaPill extends StatelessWidget {
  const _MetaPill({
    required this.icon,
    required this.label,
  });

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
      decoration: BoxDecoration(
        color: AppColors.surfaceAlt,
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: AppColors.panelBorder),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 16, color: AppColors.primary),
          const Gap(6),
          Text(label, style: Theme.of(context).textTheme.bodySmall),
        ],
      ),
    );
  }
}
