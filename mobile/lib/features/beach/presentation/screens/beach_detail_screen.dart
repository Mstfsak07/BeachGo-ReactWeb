import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:gap/gap.dart';

import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/core/theme/app_theme.dart';
import 'package:beachgo/features/beach/domain/entities/beach.dart';
import 'package:beachgo/features/beach/presentation/providers/beach_list_provider.dart';
import 'package:beachgo/shared/widgets/shared_widgets.dart';

class BeachDetailScreen extends ConsumerWidget {
  const BeachDetailScreen({
    required this.beachId,
    super.key,
  });

  final int beachId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final beachAsync = ref.watch(beachDetailProvider(beachId));

    return Scaffold(
      body: beachAsync.when(
        data: (result) {
          if (result is FailureResult<Beach>) {
            return const Center(
              child: AppErrorWidget(message: 'Plaj detayi su anda yuklenemedi.'),
            );
          }

          final beach = (result as Success<Beach>).data;

          return CustomScrollView(
            slivers: [
              SliverAppBar(
                expandedHeight: 280,
                pinned: true,
                flexibleSpace: FlexibleSpaceBar(
                  title: Text(beach.name.isNotEmpty ? beach.name : 'Plaj Detayi'),
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
                            begin: Alignment.topCenter,
                            end: Alignment.bottomCenter,
                            colors: [Colors.transparent, Colors.black54],
                          ),
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
                      Wrap(
                        spacing: 8,
                        runSpacing: 8,
                        children: [
                          _InfoChip(
                            icon: Icons.star_rounded,
                            label: '${beach.rating.toStringAsFixed(1)} puan',
                            color: Colors.amber,
                          ),
                          _InfoChip(
                            icon: Icons.people_alt_outlined,
                            label: '%${beach.occupancyPercent.toStringAsFixed(0)} dolu',
                            color: AppColors.primary,
                          ),
                          _InfoChip(
                            icon: Icons.schedule_outlined,
                            label: '${beach.openTime} - ${beach.closeTime}',
                            color: Colors.teal,
                          ),
                        ],
                      ),
                      const Gap(20),
                      if (beach.address.isNotEmpty)
                        _DetailSection(
                          title: 'Konum',
                          child: Text(
                            beach.address,
                            style: Theme.of(context).textTheme.bodyLarge,
                          ),
                        ),
                      if (beach.description.isNotEmpty)
                        _DetailSection(
                          title: 'Hakkinda',
                          child: Text(
                            beach.description,
                            style: Theme.of(context).textTheme.bodyLarge,
                          ),
                        ),
                      _DetailSection(
                        title: 'One Cikan Bilgiler',
                        child: Wrap(
                          spacing: 10,
                          runSpacing: 10,
                          children: [
                            if (beach.hasEntryFee) _StatPill('Giris ${beach.entryFee} TL'),
                            if (!beach.hasEntryFee)
                              const _StatPill('Ucretsiz giris'),
                            if (beach.sunbedPrice > 0)
                              _StatPill('Sezlong ${beach.sunbedPrice} TL'),
                            _StatPill(beach.isOpen ? 'Su an acik' : 'Su an kapali'),
                            if (beach.reviewCount > 0)
                              _StatPill('${beach.reviewCount} degerlendirme'),
                          ],
                        ),
                      ),
                      if (beach.facilities.isNotEmpty)
                        _DetailSection(
                          title: 'Olanaklar',
                          child: Wrap(
                            spacing: 8,
                            runSpacing: 8,
                            children: [
                              for (final facility in beach.facilities)
                                Chip(label: Text(facility)),
                            ],
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
          colors: [Color(0xFF2563EB), Color(0xFF0EA5E9)],
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
        color: color.withAlpha(30),
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
