import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:gap/gap.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:url_launcher/url_launcher.dart';

import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/core/router/app_router.dart';
import 'package:beachgo/core/theme/app_theme.dart';
import 'package:beachgo/features/auth/presentation/providers/auth_provider.dart';
import 'package:beachgo/features/beach/domain/entities/beach.dart';
import 'package:beachgo/features/beach/domain/entities/beach_review.dart';
import 'package:beachgo/features/beach/domain/entities/weather.dart';
import 'package:beachgo/features/beach/presentation/providers/beach_list_provider.dart';
import 'package:beachgo/features/beach/presentation/providers/review_submit_provider.dart';
import 'package:beachgo/features/favorites/presentation/providers/favorite_beaches_provider.dart';
import 'package:beachgo/features/stories/domain/entities/story.dart';
import 'package:beachgo/features/stories/presentation/providers/story_provider.dart';
import 'package:beachgo/features/stories/presentation/screens/story_viewer_screen.dart';
import 'package:beachgo/shared/widgets/shared_widgets.dart';

class BeachDetailScreen extends ConsumerWidget {
  const BeachDetailScreen({
    required this.beachId,
    super.key,
  });

  final int beachId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final isAuthenticated = ref.watch(isAuthenticatedProvider);
    final currentUser = ref.watch(currentUserProvider);
    final beachAsync = ref.watch(beachDetailProvider(beachId));
    final reviewsAsync = ref.watch(beachReviewsProvider(beachId));
    final weatherAsync = ref.watch(beachWeatherProvider(beachId));
    final storiesAsync = ref.watch(beachStoriesProvider(beachId));
    final favoriteAsync = isAuthenticated
        ? ref.watch(favoriteBeachesProvider)
        : const AsyncData<List<Beach>>(<Beach>[]);

    return Scaffold(
      bottomNavigationBar: beachAsync.when(
        data: (result) => switch (result) {
          Success<Beach>(data: final beach) => SafeArea(
              top: false,
              child: Padding(
                padding: const EdgeInsets.fromLTRB(16, 12, 16, 16),
                child: Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: AppColors.surfaceRaised,
                    borderRadius: BorderRadius.circular(24),
                    border: Border.all(color: AppColors.panelBorder),
                  ),
                  child: Row(
                    children: [
                      Expanded(
                        child: Column(
                          mainAxisSize: MainAxisSize.min,
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              beach.hasEntryFee
                                  ? 'Giris ${beach.entryFee} TL'
                                  : 'Ucretsiz giris',
                              style: Theme.of(context).textTheme.titleMedium,
                            ),
                            const Gap(2),
                            Text(
                              'Musaitlik ve detaylar rezervasyon adiminda dogrulanir.',
                              style: Theme.of(context).textTheme.bodySmall,
                            ),
                          ],
                        ),
                      ),
                      const Gap(12),
                      SizedBox(
                        height: 54,
                        child: ElevatedButton(
                          onPressed: () {
                            context.pushNamed(
                              AppRoute.reservation.name,
                              pathParameters: {'beachId': beach.id.toString()},
                              extra: beach,
                            );
                          },
                          child: const Text('Rezervasyon Yap'),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          FailureResult<Beach>() => null,
        },
        loading: () => null,
        error: (_, __) => null,
      ),
      body: beachAsync.when(
        data: (result) {
          if (result is FailureResult<Beach>) {
            return const Center(
              child: AppErrorWidget(message: 'Plaj detayi su anda yuklenemedi.'),
            );
          }

          final beach = (result as Success<Beach>).data;
          final isFavorite = favoriteAsync.valueOrNull
                  ?.any((item) => item.id == beach.id) ??
              false;

          return CustomScrollView(
            slivers: [
              SliverAppBar(
                expandedHeight: 320,
                pinned: true,
                actions: [
                  IconButton(
                    onPressed: () async {
                      if (!isAuthenticated) {
                        context.pushNamed(AppRoute.login.name);
                        return;
                      }

                      final failure = await ref
                          .read(favoriteBeachesProvider.notifier)
                          .toggleFavorite(beach);
                      if (failure != null && context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(content: Text(failure.message)),
                        );
                      }
                    },
                    icon: Icon(
                      isFavorite
                          ? Icons.favorite_rounded
                          : Icons.favorite_border_rounded,
                      color: isFavorite ? AppColors.error : Colors.white,
                    ),
                  ),
                ],
                flexibleSpace: FlexibleSpaceBar(
                  titlePadding: const EdgeInsetsDirectional.only(
                    start: 20,
                    bottom: 18,
                    end: 20,
                  ),
                  title: Text(
                    beach.name.isNotEmpty ? beach.name : 'Plaj Detayi',
                    style: Theme.of(context).textTheme.titleLarge?.copyWith(
                          color: Colors.white,
                          fontWeight: FontWeight.w800,
                        ),
                  ),
                  background: Stack(
                    fit: StackFit.expand,
                    children: [
                      if (beach.imageUrl.isNotEmpty)
                        CachedNetworkImage(
                          imageUrl: beach.imageUrl,
                          fit: BoxFit.cover,
                          errorWidget: (_, __, ___) =>
                              _DetailHeroFallback(beach.name),
                        )
                      else
                        _DetailHeroFallback(beach.name),
                      const DecoratedBox(
                        decoration: BoxDecoration(
                          gradient: LinearGradient(
                            begin: Alignment.topLeft,
                            end: Alignment.bottomCenter,
                            colors: [
                              Color(0x14000000),
                              Color(0x70000000),
                            ],
                          ),
                        ),
                      ),
                      Positioned(
                        left: 20,
                        right: 20,
                        bottom: 76,
                        child: Row(
                          children: [
                            if (beach.address.isNotEmpty)
                              Expanded(
                                child: _HeroMetaPill(
                                  icon: Icons.place_outlined,
                                  label: beach.address,
                                ),
                              ),
                            if (beach.rating > 0) ...[
                              const Gap(10),
                              _HeroMetaPill(
                                icon: Icons.star_rounded,
                                label: beach.rating.toStringAsFixed(1),
                              ),
                            ],
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ),
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(20, 20, 20, 32),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Container(
                        padding: const EdgeInsets.all(18),
                        decoration: BoxDecoration(
                          color: AppColors.surfaceRaised,
                          borderRadius: BorderRadius.circular(24),
                          border: Border.all(color: AppColors.panelBorder),
                        ),
                        child: Wrap(
                          spacing: 10,
                          runSpacing: 10,
                          children: [
                            _InfoChip(
                              icon: Icons.people_alt_outlined,
                              label:
                                  '%${beach.occupancyPercent.toStringAsFixed(0)} dolu',
                              color: AppColors.primary,
                            ),
                            _InfoChip(
                              icon: Icons.schedule_outlined,
                              label: '${beach.openTime} - ${beach.closeTime}',
                              color: Colors.teal,
                            ),
                            _InfoChip(
                              icon: beach.isOpen
                                  ? Icons.check_circle_outline
                                  : Icons.do_not_disturb_on_outlined,
                              label: beach.isOpen ? 'Su an acik' : 'Su an kapali',
                              color: beach.isOpen
                                  ? const Color(0xFF0F9D7A)
                                  : AppColors.error,
                            ),
                          ],
                        ),
                      ),
                      const Gap(20),
                      if (beach.address.isNotEmpty)
                        _DetailSection(
                          title: 'Konum',
                          child: _SectionCard(
                            child: Column(
                              children: [
                                Row(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Container(
                                      width: 42,
                                      height: 42,
                                      decoration: BoxDecoration(
                                        color: AppColors.primaryLight,
                                        borderRadius: BorderRadius.circular(14),
                                        border: Border.all(color: AppColors.panelBorder),
                                      ),
                                      child: const Icon(
                                        Icons.map_outlined,
                                        color: AppColors.primary,
                                      ),
                                    ),
                                    const Gap(12),
                                    Expanded(
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: [
                                          Text(
                                            beach.address,
                                            style: Theme.of(context)
                                                .textTheme
                                                .bodyLarge,
                                          ),
                                          const Gap(4),
                                          Text(
                                            beach.location,
                                            style: Theme.of(context)
                                                .textTheme
                                                .bodySmall,
                                          ),
                                        ],
                                      ),
                                    ),
                                  ],
                                ),
                                const Gap(14),
                                _QuickActionBar(
                                  actions: [
                                    _QuickActionItem(
                                      icon: Icons.map_outlined,
                                      label: 'Harita',
                                      onTap: () => _openMap(beach),
                                    ),
                                    _QuickActionItem(
                                      icon: Icons.route_outlined,
                                      label: 'Yol Tarifi',
                                      onTap: () => _openDirections(beach),
                                    ),
                                    if (beach.phone.isNotEmpty)
                                      _QuickActionItem(
                                        icon: Icons.call_outlined,
                                        label: 'Ara',
                                        onTap: () => _openPhone(beach.phone),
                                      ),
                                    if (beach.instagram.isNotEmpty)
                                      _QuickActionItem(
                                        icon: Icons.camera_alt_outlined,
                                        label: 'Instagram',
                                        onTap: () =>
                                            _openInstagram(beach.instagram),
                                      ),
                                    if (beach.website.isNotEmpty)
                                      _QuickActionItem(
                                        icon: Icons.language_rounded,
                                        label: 'Web',
                                        onTap: () => _openWebsite(beach.website),
                                      ),
                                    _QuickActionItem(
                                      icon: Icons.reviews_outlined,
                                      label: 'Yorumlar',
                                      onTap: () => _openGoogleReviews(beach),
                                    ),
                                    if (isAuthenticated)
                                      _QuickActionItem(
                                        icon: Icons.rate_review_outlined,
                                        label: 'Yorum Ekle',
                                        onTap: () => _openReviewSheet(
                                          context,
                                          ref,
                                          beach,
                                          currentUser?.displayName ?? 'Misafir',
                                          currentUser?.phone ?? '',
                                        ),
                                      ),
                                  ],
                                ),
                              ],
                            ),
                          ),
                        ),
                      if (beach.description.isNotEmpty)
                        _DetailSection(
                          title: 'Hakkinda',
                          child: _SectionCard(
                            child: Text(
                              beach.description,
                              style: Theme.of(context).textTheme.bodyLarge,
                            ),
                          ),
                        ),
                      _DetailSection(
                        title: 'One Cikan Bilgiler',
                        child: _SectionCard(
                          child: Wrap(
                            spacing: 10,
                            runSpacing: 10,
                            children: [
                              if (beach.hasEntryFee)
                                _StatPill('Giris ${beach.entryFee} TL'),
                              if (!beach.hasEntryFee)
                                const _StatPill('Ucretsiz giris'),
                              if (beach.sunbedPrice > 0)
                                _StatPill('Sezlong ${beach.sunbedPrice} TL'),
                              if (beach.reviewCount > 0)
                                _StatPill('${beach.reviewCount} degerlendirme'),
                              if (beach.rating > 0)
                                _StatPill(
                                  '${beach.rating.toStringAsFixed(1)} puan',
                                ),
                            ],
                          ),
                        ),
                      ),
                      _DetailSection(
                        title: 'Hava ve Deniz',
                        child: _SectionCard(
                          child: weatherAsync.when(
                            data: (result) => switch (result) {
                              Success<Weather>(data: final weather) => _WeatherSection(
                                  weather: weather,
                                  fallbackLabel: beach.isOpen
                                      ? 'Su an plaj keyfi icin uygun gorunuyor.'
                                      : 'Yola cikmadan once saatleri kontrol edin.',
                                ),
                              FailureResult<Weather>() => const _WeatherUnavailable(),
                            },
                            loading: () => const Padding(
                              padding: EdgeInsets.symmetric(vertical: 18),
                              child: Center(child: CircularProgressIndicator()),
                            ),
                            error: (_, __) => const _WeatherUnavailable(),
                          ),
                        ),
                      ),
                      storiesAsync.when(
                        data: (result) => switch (result) {
                          Success<List<Story>>(data: final stories)
                              when stories.isNotEmpty =>
                            _DetailSection(
                              title: 'Anlik Storyler',
                              child: _SectionCard(
                                child: _BeachStoryPreview(
                                  beach: beach,
                                  stories: stories,
                                ),
                              ),
                            ),
                          _ => const SizedBox.shrink(),
                        },
                        loading: () => const SizedBox.shrink(),
                        error: (_, __) => const SizedBox.shrink(),
                      ),
                      if (beach.facilities.isNotEmpty)
                        _DetailSection(
                          title: 'Olanaklar',
                          child: _SectionCard(
                            child: Wrap(
                              spacing: 8,
                              runSpacing: 8,
                              children: [
                                for (final facility in beach.facilities)
                                  Chip(label: Text(facility)),
                              ],
                            ),
                          ),
                        ),
                      _DetailSection(
                        title: 'Son Yorumlar',
                        child: _SectionCard(
                          child: reviewsAsync.when(
                            data: (result) => switch (result) {
                              Success<List<BeachReview>>(data: final reviews)
                                  when reviews.isNotEmpty =>
                                Column(
                                  children: [
                                    for (var i = 0; i < reviews.take(3).length; i++) ...[
                                      _ReviewTile(review: reviews[i]),
                                      if (i != reviews.take(3).length - 1)
                                        const Padding(
                                          padding: EdgeInsets.symmetric(vertical: 12),
                                          child: Divider(),
                                        ),
                                    ],
                                    const Gap(14),
                                    Align(
                                      alignment: Alignment.centerLeft,
                                      child: TextButton.icon(
                                        onPressed: () => _openGoogleReviews(beach),
                                        icon: const Icon(Icons.open_in_new_rounded),
                                        label: const Text('Google yorumlarini ac'),
                                      ),
                                    ),
                                    if (isAuthenticated)
                                      Align(
                                        alignment: Alignment.centerLeft,
                                        child: TextButton.icon(
                                          onPressed: () => _openReviewSheet(
                                            context,
                                            ref,
                                            beach,
                                            currentUser?.displayName ?? 'Misafir',
                                            currentUser?.phone ?? '',
                                          ),
                                          icon: const Icon(
                                            Icons.rate_review_outlined,
                                          ),
                                          label: const Text('Yorum ekle'),
                                        ),
                                      ),
                                  ],
                                ),
                              FailureResult<List<BeachReview>>() => _ReviewEmptyState(
                                  message:
                                      'Yorumlar su anda yuklenemedi. Google yorumlarini acarak devam edebilirsiniz.',
                                  onTap: () => _openGoogleReviews(beach),
                                ),
                              _ => _ReviewEmptyState(
                                  message:
                                      'Bu plaj icin henuz onaylanmis yorum bulunmuyor.',
                                  onTap: () => _openGoogleReviews(beach),
                                ),
                            },
                            loading: () => const Padding(
                              padding: EdgeInsets.symmetric(vertical: 20),
                              child: Center(child: CircularProgressIndicator()),
                            ),
                            error: (_, __) => _ReviewEmptyState(
                              message:
                                  'Yorumlar su anda yuklenemedi. Google yorumlarini acarak devam edebilirsiniz.',
                              onTap: () => _openGoogleReviews(beach),
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          );
        },
        loading: () => const FullScreenLoader(),
        error: (_, __) => const Center(
          child: AppErrorWidget(message: 'Plaj detayi yuklenemedi.'),
        ),
      ),
    );
  }

  Future<void> _openMap(Beach beach) async {
    final hasCoordinates = beach.latitude != null && beach.longitude != null;
    final query = hasCoordinates
        ? '${beach.latitude},${beach.longitude}'
        : '${beach.name} ${beach.location}'.trim();
    final uri = Uri.parse(
      'https://www.google.com/maps/search/?api=1&query=${Uri.encodeComponent(query)}',
    );
    await launchUrl(uri, mode: LaunchMode.externalApplication);
  }

  Future<void> _openDirections(Beach beach) async {
    final hasCoordinates = beach.latitude != null && beach.longitude != null;
    final destination = hasCoordinates
        ? '${beach.latitude},${beach.longitude}'
        : '${beach.name} ${beach.location}'.trim();
    final uri = Uri.parse(
      'https://www.google.com/maps/dir/?api=1&destination=${Uri.encodeComponent(destination)}',
    );
    await launchUrl(uri, mode: LaunchMode.externalApplication);
  }

  Future<void> _openGoogleReviews(Beach beach) async {
    final query = '${beach.name} ${beach.location} yorumlar'.trim();
    final uri = Uri.parse(
      'https://www.google.com/search?q=${Uri.encodeComponent(query)}',
    );
    await launchUrl(uri, mode: LaunchMode.externalApplication);
  }

  Future<void> _openPhone(String phone) async {
    final sanitized = phone.replaceAll(' ', '');
    final uri = Uri.parse('tel:$sanitized');
    await launchUrl(uri, mode: LaunchMode.externalApplication);
  }

  Future<void> _openWebsite(String website) async {
    final normalized = website.startsWith('http')
        ? website
        : 'https://$website';
    final uri = Uri.parse(normalized);
    await launchUrl(uri, mode: LaunchMode.externalApplication);
  }

  Future<void> _openInstagram(String instagram) async {
    final handle = instagram.replaceAll('@', '').trim();
    final normalized = instagram.startsWith('http')
        ? instagram
        : 'https://instagram.com/$handle';
    final uri = Uri.parse(normalized);
    await launchUrl(uri, mode: LaunchMode.externalApplication);
  }

  Future<void> _openReviewSheet(
    BuildContext context,
    WidgetRef ref,
    Beach beach,
    String userName,
    String userPhone,
  ) async {
    await showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      showDragHandle: true,
      builder: (context) => Padding(
        padding: EdgeInsets.only(
          left: 20,
          right: 20,
          top: 12,
          bottom: 20 + MediaQuery.of(context).viewInsets.bottom,
        ),
        child: _ReviewComposer(
          beachId: beach.id,
          beachName: beach.name,
          userName: userName,
          userPhone: userPhone,
          onSubmitted: () {
            ref.invalidate(beachReviewsProvider(beach.id));
            ref.invalidate(beachDetailProvider(beach.id));
          },
        ),
      ),
    );
  }
}

class _DetailHeroFallback extends StatelessWidget {
  const _DetailHeroFallback(this.name);

  final String name;

  @override
  Widget build(BuildContext context) {
    final initial = name.isNotEmpty ? name[0].toUpperCase() : 'B';

    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          colors: [AppColors.primaryDark, AppColors.primary],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
      ),
      child: Center(
        child: Text(
          initial,
          style: const TextStyle(
            color: Colors.white,
            fontSize: 72,
            fontWeight: FontWeight.w900,
          ),
        ),
      ),
    );
  }
}

class _DetailSection extends StatelessWidget {
  const _DetailSection({
    required this.title,
    required this.child,
  });

  final String title;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(title, style: Theme.of(context).textTheme.titleLarge),
          const Gap(10),
          child,
        ],
      ),
    );
  }
}

class _InfoChip extends StatelessWidget {
  const _InfoChip({
    required this.icon,
    required this.label,
    required this.color,
  });

  final IconData icon;
  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.10),
        borderRadius: BorderRadius.circular(999),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 18, color: color),
          const Gap(6),
          Text(
            label,
            style: TextStyle(color: color, fontWeight: FontWeight.w700),
          ),
        ],
      ),
    );
  }
}

class _StatPill extends StatelessWidget {
  const _StatPill(this.label);

  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: AppColors.slate100,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        label,
        style: Theme.of(context).textTheme.bodyMedium?.copyWith(
              fontWeight: FontWeight.w700,
              color: AppColors.slate700,
            ),
      ),
    );
  }
}

class _SectionCard extends StatelessWidget {
  const _SectionCard({required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: AppColors.surfaceRaised,
        borderRadius: BorderRadius.circular(22),
        border: Border.all(color: AppColors.panelBorder),
      ),
      child: child,
    );
  }
}

class _HeroMetaPill extends StatelessWidget {
  const _HeroMetaPill({
    required this.icon,
    required this.label,
  });

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: Colors.black.withValues(alpha: 0.36),
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: Colors.white.withValues(alpha: 0.12)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 16, color: Colors.white),
          const Gap(6),
          Flexible(
            child: Text(
              label,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.w700,
                  ),
            ),
          ),
        ],
      ),
    );
  }
}

class _ReviewTile extends StatelessWidget {
  const _ReviewTile({required this.review});

  final BeachReview review;

  @override
  Widget build(BuildContext context) {
    final dateLabel = DateFormat('d MMM y', 'tr_TR').format(review.createdAt);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Container(
              width: 38,
              height: 38,
              decoration: BoxDecoration(
                color: AppColors.primaryLight,
                borderRadius: BorderRadius.circular(12),
              ),
              child: Center(
                child: Text(
                  review.userName.isNotEmpty
                      ? review.userName[0].toUpperCase()
                      : 'M',
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        color: AppColors.primary,
                      ),
                ),
              ),
            ),
            const Gap(12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(review.userName, style: Theme.of(context).textTheme.titleMedium),
                  Text(dateLabel, style: Theme.of(context).textTheme.bodySmall),
                ],
              ),
            ),
            _RatingPill(rating: review.rating),
          ],
        ),
        if (review.comment.isNotEmpty) ...[
          const Gap(10),
          Text(review.comment, style: Theme.of(context).textTheme.bodyMedium),
        ],
      ],
    );
  }
}

class _ReviewEmptyState extends StatelessWidget {
  const _ReviewEmptyState({
    required this.message,
    required this.onTap,
  });

  final String message;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(message, style: Theme.of(context).textTheme.bodyMedium),
        const Gap(12),
        OutlinedButton.icon(
          onPressed: onTap,
          icon: const Icon(Icons.open_in_new_rounded),
          label: const Text('Google yorumlarini ac'),
        ),
      ],
    );
  }
}

class _RatingPill extends StatelessWidget {
  const _RatingPill({required this.rating});

  final int rating;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: AppColors.accent,
        borderRadius: BorderRadius.circular(999),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(Icons.star_rounded, size: 14, color: Color(0xFF1A0D00)),
          const Gap(4),
          Text(
            rating.toString(),
            style: Theme.of(context).textTheme.bodySmall?.copyWith(
                  color: const Color(0xFF1A0D00),
                  fontWeight: FontWeight.w800,
                ),
          ),
        ],
      ),
    );
  }
}

class _WeatherSection extends StatelessWidget {
  const _WeatherSection({
    required this.weather,
    required this.fallbackLabel,
  });

  final Weather weather;
  final String fallbackLabel;

  @override
  Widget build(BuildContext context) {
    final description = weather.description.trim().isEmpty
        ? fallbackLabel
        : weather.description;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Wrap(
          spacing: 10,
          runSpacing: 10,
          children: [
            _WeatherStat(
              icon: Icons.thermostat_rounded,
              label: 'Hava',
              value: weather.temperature == null
                  ? '--'
                  : '${weather.temperature!.round()}\u00B0',
              highlightColor: AppColors.primary,
            ),
            _WeatherStat(
              icon: Icons.water_drop_outlined,
              label: 'Deniz',
              value: weather.seaTemperature == null
                  ? '--'
                  : '${weather.seaTemperature!.round()}\u00B0',
              highlightColor: AppColors.sea,
            ),
            _WeatherStat(
              icon: Icons.air_rounded,
              label: 'Ruzgar',
              value: weather.windSpeed == null
                  ? '--'
                  : '${weather.windSpeed!.toStringAsFixed(1)} m/s',
              highlightColor: AppColors.accent,
            ),
            _WeatherStat(
              icon: Icons.waves_rounded,
              label: 'Dalga',
              value: weather.waveHeight == null
                  ? '--'
                  : '${weather.waveHeight!.toStringAsFixed(1)} m',
              highlightColor: AppColors.success,
            ),
          ],
        ),
        const Gap(14),
        Text(
          description,
          style: Theme.of(context).textTheme.bodyMedium,
        ),
      ],
    );
  }
}

class _WeatherStat extends StatelessWidget {
  const _WeatherStat({
    required this.icon,
    required this.label,
    required this.value,
    required this.highlightColor,
  });

  final IconData icon;
  final String label;
  final String value;
  final Color highlightColor;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 140,
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppColors.surfaceAlt,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: AppColors.panelBorder),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, color: highlightColor, size: 18),
          const Gap(10),
          Text(value, style: Theme.of(context).textTheme.titleMedium),
          const Gap(2),
          Text(label, style: Theme.of(context).textTheme.bodySmall),
        ],
      ),
    );
  }
}

class _WeatherUnavailable extends StatelessWidget {
  const _WeatherUnavailable();

  @override
  Widget build(BuildContext context) {
    return Text(
      'Hava ve deniz verisi su an alinamiyor. Birazdan tekrar kontrol edin.',
      style: Theme.of(context).textTheme.bodyMedium,
    );
  }
}

class _QuickActionItem {
  const _QuickActionItem({
    required this.icon,
    required this.label,
    required this.onTap,
  });

  final IconData icon;
  final String label;
  final VoidCallback onTap;
}

class _QuickActionBar extends StatelessWidget {
  const _QuickActionBar({required this.actions});

  final List<_QuickActionItem> actions;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 94,
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        itemCount: actions.length,
        separatorBuilder: (_, __) => const Gap(10),
        itemBuilder: (context, index) {
          final action = actions[index];
          return InkWell(
            onTap: action.onTap,
            borderRadius: BorderRadius.circular(18),
            child: Ink(
              width: 92,
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
              decoration: BoxDecoration(
                color: AppColors.surfaceAlt,
                borderRadius: BorderRadius.circular(18),
                border: Border.all(color: AppColors.panelBorder),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Container(
                    width: 40,
                    height: 40,
                    decoration: BoxDecoration(
                      color: AppColors.primaryLight,
                      borderRadius: BorderRadius.circular(14),
                    ),
                    child: Icon(action.icon, color: AppColors.primary, size: 20),
                  ),
                  const Gap(10),
                  Text(
                    action.label,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    textAlign: TextAlign.center,
                    style: Theme.of(context).textTheme.bodySmall?.copyWith(
                          color: AppColors.slate700,
                          fontWeight: FontWeight.w700,
                        ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}

class _BeachStoryPreview extends StatelessWidget {
  const _BeachStoryPreview({
    required this.beach,
    required this.stories,
  });

  final Beach beach;
  final List<Story> stories;

  @override
  Widget build(BuildContext context) {
    final sortedStories = List<Story>.from(stories)
      ..sort((a, b) => a.createdAt.compareTo(b.createdAt));
    final coverStory = sortedStories.first;
    final group = StoryBeachGroup(
      beachId: beach.id,
      beachName: beach.name,
      coverImageUrl: coverStory.beachImageUrl.isNotEmpty
          ? coverStory.beachImageUrl
          : coverStory.mediaUrl,
      stories: List<Story>.unmodifiable(sortedStories),
    );

    return InkWell(
      onTap: () {
        Navigator.of(context).push(
          MaterialPageRoute<void>(
            builder: (_) => StoryViewerScreen(group: group),
            fullscreenDialog: true,
          ),
        );
      },
      borderRadius: BorderRadius.circular(22),
      child: Ink(
        decoration: BoxDecoration(
          color: AppColors.surfaceAlt,
          borderRadius: BorderRadius.circular(22),
          border: Border.all(color: AppColors.panelBorder),
        ),
        child: Row(
          children: [
            ClipRRect(
              borderRadius: const BorderRadius.horizontal(
                left: Radius.circular(22),
              ),
              child: SizedBox(
                width: 112,
                height: 116,
                child: coverStory.beachImageUrl.isNotEmpty ||
                        coverStory.mediaUrl.isNotEmpty
                    ? CachedNetworkImage(
                        imageUrl: coverStory.beachImageUrl.isNotEmpty
                            ? coverStory.beachImageUrl
                            : coverStory.mediaUrl,
                        fit: BoxFit.cover,
                        errorWidget: (_, __, ___) => _StoryPreviewFallback(
                          label: beach.name,
                        ),
                      )
                    : _StoryPreviewFallback(label: beach.name),
              ),
            ),
            Expanded(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 10,
                        vertical: 6,
                      ),
                      decoration: BoxDecoration(
                        color: AppColors.primaryLight,
                        borderRadius: BorderRadius.circular(999),
                      ),
                      child: Text(
                        '${stories.length} story hazir',
                        style: Theme.of(context).textTheme.bodySmall?.copyWith(
                              color: AppColors.primary,
                              fontWeight: FontWeight.w700,
                            ),
                      ),
                    ),
                    const Gap(12),
                    Text(
                      'Plajin son anlarini izle',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const Gap(6),
                    Text(
                      coverStory.caption.isNotEmpty
                          ? coverStory.caption
                          : 'Guncel atmosfer, kalabalik ve sahil anlari burada.',
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style: Theme.of(context).textTheme.bodySmall,
                    ),
                    const Gap(12),
                    Row(
                      children: [
                        const Icon(
                          Icons.play_circle_outline_rounded,
                          color: AppColors.primary,
                          size: 18,
                        ),
                        const Gap(6),
                        Text(
                          'Storyleri ac',
                          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                color: AppColors.primary,
                                fontWeight: FontWeight.w700,
                              ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _StoryPreviewFallback extends StatelessWidget {
  const _StoryPreviewFallback({required this.label});

  final String label;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          colors: [AppColors.primaryDark, AppColors.primary],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
      ),
      child: Center(
        child: Text(
          label.isNotEmpty ? label[0].toUpperCase() : 'S',
          style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                color: Colors.white,
              ),
        ),
      ),
    );
  }
}

class _ReviewComposer extends ConsumerStatefulWidget {
  const _ReviewComposer({
    required this.beachId,
    required this.beachName,
    required this.userName,
    required this.userPhone,
    required this.onSubmitted,
  });

  final int beachId;
  final String beachName;
  final String userName;
  final String userPhone;
  final VoidCallback onSubmitted;

  @override
  ConsumerState<_ReviewComposer> createState() => _ReviewComposerState();
}

class _ReviewComposerState extends ConsumerState<_ReviewComposer> {
  final _commentController = TextEditingController();
  int _rating = 5;

  @override
  void dispose() {
    _commentController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final submitState = ref.watch(reviewSubmitProvider(widget.beachId));

    return SafeArea(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Yorum Ekle', style: Theme.of(context).textTheme.titleLarge),
          const Gap(6),
          Text(
            widget.beachName,
            style: Theme.of(context).textTheme.bodyMedium,
          ),
          const Gap(16),
          Row(
            children: List.generate(5, (index) {
              final value = index + 1;
              return IconButton(
                onPressed: () => setState(() => _rating = value),
                icon: Icon(
                  value <= _rating ? Icons.star_rounded : Icons.star_outline_rounded,
                  color: AppColors.accent,
                ),
              );
            }),
          ),
          TextField(
            controller: _commentController,
            minLines: 3,
            maxLines: 5,
            decoration: const InputDecoration(
              labelText: 'Yorumunuz',
              alignLabelWithHint: true,
            ),
          ),
          if (submitState.errorMessage != null) ...[
            const Gap(12),
            Text(
              submitState.errorMessage!,
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: AppColors.error,
                    fontWeight: FontWeight.w700,
                  ),
            ),
          ],
          const Gap(16),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: submitState.isSubmitting
                  ? null
                  : () async {
                      final navigator = Navigator.of(context);
                      final messenger = ScaffoldMessenger.of(context);
                      final failure = await ref
                          .read(reviewSubmitProvider(widget.beachId).notifier)
                          .submit(
                            userName: widget.userName,
                            userPhone: widget.userPhone,
                            rating: _rating,
                            comment: _commentController.text,
                          );
                      if (failure == null && mounted) {
                        widget.onSubmitted();
                        navigator.pop();
                        messenger.showSnackBar(
                          const SnackBar(
                            content: Text('Yorumunuz gonderildi.'),
                          ),
                        );
                      }
                    },
              child: submitState.isSubmitting
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2.2),
                    )
                  : const Text('Yorumu Gonder'),
            ),
          ),
        ],
      ),
    );
  }
}
