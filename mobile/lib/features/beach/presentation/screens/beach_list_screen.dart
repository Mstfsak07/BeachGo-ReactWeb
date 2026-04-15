import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:gap/gap.dart';
import 'package:go_router/go_router.dart';

import 'package:beachgo/core/theme/app_theme.dart';
import 'package:beachgo/features/beach/domain/entities/beach.dart';
import 'package:beachgo/features/beach/domain/entities/beach_filter.dart';
import 'package:beachgo/features/beach/presentation/providers/beach_list_provider.dart';
import 'package:beachgo/features/stories/presentation/widgets/story_strip.dart';
import 'package:beachgo/shared/widgets/shared_widgets.dart';

class BeachListScreen extends ConsumerStatefulWidget {
  const BeachListScreen({super.key});

  @override
  ConsumerState<BeachListScreen> createState() => _BeachListScreenState();
}

class _BeachListScreenState extends ConsumerState<BeachListScreen> {
  late final TextEditingController _searchController;
  late final ScrollController _scrollController;

  static const Map<String, BeachFilter> _categoryFilters = {
    'Populer': BeachFilter(sortBy: 'rating'),
    'Aile': BeachFilter(sortBy: 'rating', isChildFriendly: true),
    'Parti': BeachFilter(sortBy: 'rating', hasBar: true),
    'Luks': BeachFilter(sortBy: 'rating', hasPool: true),
    'Ucretsiz': BeachFilter(sortBy: 'rating', freeEntry: true),
  };

  @override
  void initState() {
    super.initState();
    _searchController = TextEditingController();
    _scrollController = ScrollController()..addListener(_handleScroll);
  }

  @override
  void dispose() {
    _scrollController
      ..removeListener(_handleScroll)
      ..dispose();
    _searchController.dispose();
    super.dispose();
  }

  void _handleScroll() {
    if (!_scrollController.hasClients) {
      return;
    }

    final position = _scrollController.position;
    if (position.pixels >= position.maxScrollExtent - 500) {
      ref.read(beachListControllerProvider.notifier).loadMore();
    }
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(beachListControllerProvider);

    ref.listen<BeachListState>(beachListControllerProvider, (previous, next) {
      if (_searchController.text != next.query) {
        _searchController.text = next.query;
      }

      final previousMessage = previous?.errorMessage;
      final nextMessage = next.errorMessage;
      final shouldShowSnackBar = nextMessage != null &&
          next.items.isNotEmpty &&
          previousMessage != nextMessage;

      if (shouldShowSnackBar) {
        final friendlyMessage = _friendlyErrorMessage(nextMessage);
        ScaffoldMessenger.of(context)
          ..hideCurrentSnackBar()
          ..showSnackBar(SnackBar(content: Text(friendlyMessage)));
      }
    });

    return Scaffold(
      body: SafeArea(
        child: state.isInitialLoading && state.items.isEmpty
            ? ListView.separated(
            padding: const EdgeInsets.fromLTRB(20, 24, 20, 28),
            itemBuilder: (_, __) => const SizedBox(
              height: 318,
              child: BeachCardSkeleton(),
            ),
            separatorBuilder: (_, __) => const SizedBox(height: 18),
            itemCount: 4,
          )
            : RefreshIndicator(
                onRefresh: () =>
                    ref.read(beachListControllerProvider.notifier).refresh(),
                child: CustomScrollView(
                  controller: _scrollController,
                  physics: const AlwaysScrollableScrollPhysics(),
                  slivers: [
                    SliverToBoxAdapter(
                      child: Padding(
                        padding: const EdgeInsets.fromLTRB(20, 24, 20, 12),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            _HomeHero(
                              totalCount: state.totalCount,
                              child: const StoryStrip(),
                            ),
                            const Gap(18),
                            _SearchBar(
                              controller: _searchController,
                              activeFilterCount: state.activeFilterCount,
                              onSubmit: (value) => ref
                                  .read(beachListControllerProvider.notifier)
                                  .search(value),
                              onOpenFilter: () => _openFilterSheet(context, state),
                              onClear: () => ref
                                  .read(beachListControllerProvider.notifier)
                                  .clearFilters(),
                            ),
                            const Gap(18),
                            SizedBox(
                              height: 42,
                              child: ListView.separated(
                                scrollDirection: Axis.horizontal,
                                itemBuilder: (context, index) {
                                  final entry =
                                      _categoryFilters.entries.elementAt(index);
                                  final isActive =
                                      state.activeCategory == entry.key;
                                  return ChoiceChip(
                                    label: Text(
                                      entry.key,
                                      style: Theme.of(context)
                                          .textTheme
                                          .bodyMedium
                                          ?.copyWith(
                                            color: isActive
                                                ? Colors.white
                                                : AppColors.slate700,
                                            fontWeight: FontWeight.w700,
                                          ),
                                    ),
                                    selected: isActive,
                                    onSelected: (_) {
                                      ref
                                          .read(
                                            beachListControllerProvider.notifier,
                                          )
                                          .applyFilters(
                                            entry.value,
                                            activeCategory: entry.key,
                                          );
                                    },
                                  );
                                },
                                separatorBuilder: (_, __) => const Gap(8),
                                itemCount: _categoryFilters.length,
                              ),
                            ),
                            if (state.errorMessage != null &&
                                state.items.isNotEmpty) ...[
                              const Gap(14),
                              _BeachListInlineError(
                                message: _friendlyErrorMessage(state.errorMessage!),
                              ),
                            ],
                            const Gap(12),
                            Text(
                              '${state.totalCount} plaj kesif icin hazir',
                              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                    color: AppColors.slate700,
                                  ),
                            ),
                          ],
                        ),
                      ),
                    ),
                    if (state.showInitialError)
                      SliverFillRemaining(
                        hasScrollBody: false,
                        child: Center(
                          child: AppErrorWidget(
                            message: state.errorMessage == null
                                ? 'Plajlar yuklenemedi.'
                                : _friendlyErrorMessage(state.errorMessage!),
                            onRetry: () => ref
                                .read(beachListControllerProvider.notifier)
                                .loadInitial(),
                          ),
                        ),
                      )
                    else if (state.showEmptyState)
                      SliverFillRemaining(
                        hasScrollBody: false,
                        child: Center(
                          child: AppErrorWidget(
                            message: state.query.isNotEmpty || state.hasActiveFilters
                                ? 'Bu kriterlerle eslesen plaj bulunamadi. Aramayi veya filtreleri temizleyip tekrar deneyin.'
                                : 'Henuz listelenecek plaj bulunmuyor.',
                          ),
                        ),
                      )
                    else
                      SliverPadding(
                        padding: const EdgeInsets.fromLTRB(20, 6, 20, 28),
                        sliver: SliverList.separated(
                          itemCount: state.items.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 18),
                          itemBuilder: (context, index) {
                            final beach = state.items[index];
                            return _BeachCard(
                              beach: beach,
                              onTap: () {
                                context.push('/beaches/${beach.id}');
                              },
                            );
                          },
                        ),
                      ),
                    if (state.isLoadingMore)
                      const SliverToBoxAdapter(
                        child: Padding(
                          padding: EdgeInsets.fromLTRB(20, 0, 20, 24),
                          child: Center(child: CircularProgressIndicator()),
                        ),
                      ),
                  ],
                ),
              ),
      ),
    );
  }

  Future<void> _openFilterSheet(
    BuildContext context,
    BeachListState state,
  ) async {
    var selected = state.filters;

    await showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      showDragHandle: true,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setModalState) {
            return SafeArea(
              child: Padding(
                padding: EdgeInsets.fromLTRB(
                  20,
                  12,
                  20,
                  20 + MediaQuery.of(context).viewInsets.bottom,
                ),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Filtreler', style: Theme.of(context).textTheme.titleLarge),
                    const Gap(16),
                    Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: [
                        _FilterToggle(
                          label: 'Wifi',
                          selected: selected.hasWifi == true,
                          onTap: () => setModalState(() {
                            selected = selected.copyWith(
                              hasWifi: selected.hasWifi == true ? null : true,
                            );
                          }),
                        ),
                        _FilterToggle(
                          label: 'Ucretsiz Giris',
                          selected: selected.freeEntry == true,
                          onTap: () => setModalState(() {
                            selected = selected.copyWith(
                              freeEntry: selected.freeEntry == true ? null : true,
                            );
                          }),
                        ),
                        _FilterToggle(
                          label: 'Bar',
                          selected: selected.hasBar == true,
                          onTap: () => setModalState(() {
                            selected = selected.copyWith(
                              hasBar: selected.hasBar == true ? null : true,
                            );
                          }),
                        ),
                        _FilterToggle(
                          label: 'Su Sporlari',
                          selected: selected.hasWaterSports == true,
                          onTap: () => setModalState(() {
                            selected = selected.copyWith(
                              hasWaterSports: selected.hasWaterSports == true
                                  ? null
                                  : true,
                            );
                          }),
                        ),
                        _FilterToggle(
                          label: 'Cocuk Dostu',
                          selected: selected.isChildFriendly == true,
                          onTap: () => setModalState(() {
                            selected = selected.copyWith(
                              isChildFriendly:
                                  selected.isChildFriendly == true ? null : true,
                            );
                          }),
                        ),
                        _FilterToggle(
                          label: 'Havuz',
                          selected: selected.hasPool == true,
                          onTap: () => setModalState(() {
                            selected = selected.copyWith(
                              hasPool: selected.hasPool == true ? null : true,
                            );
                          }),
                        ),
                      ],
                    ),
                    const Gap(20),
                    Text(
                      'Minimum Puan',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const Gap(8),
                    Wrap(
                      spacing: 8,
                      children: [1, 2, 3, 4, 5].map((value) {
                        final selectedValue = selected.minRating == value.toDouble();
                        return ChoiceChip(
                          label: Text('$value+'),
                          selected: selectedValue,
                          onSelected: (_) => setModalState(() {
                            selected = selected.copyWith(
                              minRating: selectedValue ? null : value.toDouble(),
                            );
                          }),
                        );
                      }).toList(),
                    ),
                    const Gap(20),
                    Text('Siralama', style: Theme.of(context).textTheme.titleMedium),
                    const Gap(8),
                    Wrap(
                      spacing: 8,
                      children: [
                        ChoiceChip(
                          label: const Text('En yuksek puan'),
                          selected: selected.sortBy == 'rating',
                          onSelected: (_) => setModalState(() {
                            selected = selected.copyWith(sortBy: 'rating');
                          }),
                        ),
                        ChoiceChip(
                          label: const Text('En az dolu'),
                          selected: selected.sortBy == 'occupancy',
                          onSelected: (_) => setModalState(() {
                            selected = selected.copyWith(sortBy: 'occupancy');
                          }),
                        ),
                      ],
                    ),
                    const Gap(24),
                    Row(
                      children: [
                        Expanded(
                          child: OutlinedButton(
                            onPressed: () {
                              ref
                                  .read(beachListControllerProvider.notifier)
                                  .clearFilters();
                              Navigator.pop(context);
                            },
                            child: const Text('Temizle'),
                          ),
                        ),
                        const Gap(12),
                        Expanded(
                          child: ElevatedButton(
                            onPressed: () {
                              ref
                                  .read(beachListControllerProvider.notifier)
                                  .applyFilters(selected);
                              Navigator.pop(context);
                            },
                            child: const Text('Uygula'),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            );
          },
        );
      },
    );
  }
}

class _BeachListInlineError extends StatelessWidget {
  const _BeachListInlineError({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: const Color(0x33FF6B6B),
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: AppColors.error.withValues(alpha: 0.35)),
      ),
      child: Row(
        children: [
          const Icon(
            Icons.error_outline,
            color: Color(0xFFB45309),
          ),
          const Gap(10),
          Expanded(
            child: Text(
              message,
              style: Theme.of(context).textTheme.bodyMedium,
            ),
          ),
        ],
      ),
    );
  }
}

class _SearchBar extends StatelessWidget {
  const _SearchBar({
    required this.controller,
    required this.activeFilterCount,
    required this.onSubmit,
    required this.onOpenFilter,
    required this.onClear,
  });

  final TextEditingController controller;
  final int activeFilterCount;
  final ValueChanged<String> onSubmit;
  final VoidCallback onOpenFilter;
  final VoidCallback onClear;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(10),
      decoration: BoxDecoration(
        color: AppColors.surfaceRaised,
        borderRadius: BorderRadius.circular(22),
        border: Border.all(color: AppColors.slate200, width: 0.8),
      ),
      child: Row(
        children: [
          Expanded(
            child: ValueListenableBuilder<TextEditingValue>(
              valueListenable: controller,
              builder: (context, value, _) {
                return TextField(
                  controller: controller,
                  textInputAction: TextInputAction.search,
                  onSubmitted: onSubmit,
                  decoration: InputDecoration(
                    hintText: 'Plaj, konum veya atmosfer ara',
                    prefixIcon: const Icon(Icons.search),
                    suffixIcon: value.text.isEmpty
                        ? null
                        : IconButton(
                            onPressed: () {
                              controller.clear();
                              onClear();
                            },
                            icon: const Icon(Icons.close),
                          ),
                  ),
                );
              },
            ),
          ),
          const Gap(10),
          Badge(
            isLabelVisible: activeFilterCount > 0,
            label: Text('$activeFilterCount'),
            child: OutlinedButton.icon(
              onPressed: onOpenFilter,
              icon: const Icon(Icons.tune_rounded),
              label: const Text('Filtre'),
            ),
          ),
        ],
      ),
    );
  }
}

class _FilterToggle extends StatelessWidget {
  const _FilterToggle({
    required this.label,
    required this.selected,
    required this.onTap,
  });

  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return FilterChip(
      label: Text(label),
      selected: selected,
      onSelected: (_) => onTap(),
    );
  }
}

class _BeachCard extends StatelessWidget {
  const _BeachCard({
    required this.beach,
    required this.onTap,
  });

  final Beach beach;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final occupancy = beach.occupancyPercent;
    final activeFacilities = _facilityMeta.entries
        .where((entry) => _facilityValue(beach, entry.key))
        .take(5)
        .toList();

    return InkWell(
      borderRadius: BorderRadius.circular(24),
      onTap: onTap,
      child: Ink(
        decoration: BoxDecoration(
          color: AppColors.surfaceRaised,
          borderRadius: BorderRadius.circular(24),
          border: Border.all(color: AppColors.panelBorder),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            ClipRRect(
              borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
              child: Stack(
                children: [
                  SizedBox(
                    height: 210,
                    width: double.infinity,
                    child: beach.imageUrl.isNotEmpty
                        ? CachedNetworkImage(
                            imageUrl: beach.imageUrl,
                            fit: BoxFit.cover,
                            errorWidget: (_, __, ___) => _CardFallback(beach),
                          )
                        : _CardFallback(beach),
                  ),
                  Positioned(
                    top: 14,
                    left: 14,
                    right: 14,
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            _StatusBadge(
                              label: beach.hasEntryFee ? 'Giris ucretli' : 'Ucretsiz',
                              color: beach.hasEntryFee
                                  ? AppColors.warning
                                  : AppColors.primary,
                            ),
                            const Gap(6),
                            _StatusBadge(
                              label: beach.isOpen ? 'Acik' : 'Kapali',
                              color: beach.isOpen ? AppColors.success : AppColors.error,
                            ),
                          ],
                        ),
                        if (beach.rating > 0)
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 10,
                              vertical: 8,
                            ),
                            decoration: BoxDecoration(
                              color: Colors.white.withValues(alpha: 0.12),
                              borderRadius: BorderRadius.circular(18),
                              border: Border.all(
                                color: Colors.white.withValues(alpha: 0.15),
                              ),
                            ),
                            child: Row(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                const Icon(
                                  Icons.star_rounded,
                                  color: Colors.amber,
                                  size: 18,
                                ),
                                const Gap(4),
                                Text(
                                  beach.rating.toStringAsFixed(1),
                                  style: Theme.of(context)
                                      .textTheme
                                      .bodyMedium
                                      ?.copyWith(fontWeight: FontWeight.w800),
                                ),
                              ],
                            ),
                          ),
                      ],
                    ),
                  ),
                  Positioned(
                    left: 14,
                    right: 14,
                    bottom: 14,
                    child: Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: AppColors.surface.withValues(alpha: 0.84),
                        borderRadius: BorderRadius.circular(18),
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text(
                                'Canli doluluk',
                                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                                      color: Colors.white70,
                                      fontWeight: FontWeight.w700,
                                    ),
                              ),
                              Text(
                                '%$occupancy',
                                style: const TextStyle(
                                  color: Colors.white,
                                  fontWeight: FontWeight.w800,
                                ),
                              ),
                            ],
                          ),
                          const Gap(8),
                          ClipRRect(
                            borderRadius: BorderRadius.circular(999),
                            child: LinearProgressIndicator(
                              minHeight: 8,
                              value: occupancy / 100,
                              color: occupancy < 50
                                  ? AppColors.success
                                  : occupancy < 80
                                      ? AppColors.warning
                                      : AppColors.error,
                              backgroundColor: Colors.white24,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Expanded(
                        child: Text(
                          beach.name.isNotEmpty ? beach.name : 'Adsiz Plaj',
                          style: Theme.of(context)
                              .textTheme
                              .headlineMedium
                              ?.copyWith(fontSize: 21),
                        ),
                      ),
                      const Gap(12),
                      const Icon(
                        Icons.arrow_outward_rounded,
                        color: AppColors.slate400,
                        size: 20,
                      ),
                    ],
                  ),
                  const Gap(8),
                  if (beach.address.isNotEmpty)
                    Padding(
                      padding: const EdgeInsets.only(bottom: 8),
                      child: Row(
                        children: [
                          const Icon(
                            Icons.place_outlined,
                            size: 16,
                            color: AppColors.primary,
                          ),
                          const Gap(6),
                          Expanded(
                            child: Text(
                              beach.address,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                                    color: AppColors.primary,
                                    fontWeight: FontWeight.w700,
                                  ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  if (beach.description.isNotEmpty)
                    Padding(
                      padding: const EdgeInsets.only(top: 8),
                      child: Text(
                        beach.description,
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                        style: Theme.of(context).textTheme.bodyMedium,
                      ),
                    ),
                  if (activeFacilities.isNotEmpty) ...[
                    const Gap(14),
                    Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: [
                        for (final facility in activeFacilities)
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 10,
                              vertical: 8,
                            ),
                            decoration: BoxDecoration(
                              color: facility.value.$2.withValues(alpha: 0.10),
                              borderRadius: BorderRadius.circular(999),
                            ),
                            child: Icon(
                              facility.value.$1,
                              size: 16,
                              color: facility.value.$2,
                            ),
                          ),
                      ],
                    ),
                  ],
                  const Gap(16),
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          '${beach.openTime} - ${beach.closeTime}',
                          style: Theme.of(context).textTheme.bodySmall,
                        ),
                      ),
                      if (beach.reviewCount > 0)
                        Text(
                          '${beach.reviewCount} degerlendirme',
                          style: Theme.of(context).textTheme.bodySmall,
                        ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  bool _facilityValue(Beach beach, String key) {
    switch (key) {
      case 'hasBar':
        return beach.hasBar;
      case 'hasPool':
        return beach.hasPool;
      case 'hasWifi':
        return beach.hasWifi;
      case 'hasParking':
        return beach.hasParking;
      case 'hasRestaurant':
        return beach.hasRestaurant;
      case 'hasWaterSports':
        return beach.hasWaterSports;
      case 'isChildFriendly':
        return beach.isChildFriendly;
      case 'hasSunbeds':
        return beach.hasSunbeds;
      case 'hasShower':
        return beach.hasShower;
      case 'hasDJ':
        return beach.hasDJ;
      default:
        return false;
    }
  }
}

class _HomeHero extends StatelessWidget {
  const _HomeHero({
    required this.totalCount,
    required this.child,
  });

  final int totalCount;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.fromLTRB(20, 22, 20, 20),
        decoration: BoxDecoration(
          gradient: const LinearGradient(
          colors: [AppColors.surfaceAlt, AppColors.primaryDark],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(28),
        border: Border.all(color: AppColors.panelBorder),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Plajlari Kesfet',
            style: Theme.of(context).textTheme.headlineLarge?.copyWith(
                  color: Colors.white,
                ),
          ),
          const Gap(8),
          Text(
            'Gunluk atmosferi, doluluk bilgilerini ve sahil hikayelerini tek yerden takip edin.',
            style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                  color: Colors.white.withValues(alpha: 0.88),
                ),
          ),
          const Gap(16),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
            decoration: BoxDecoration(
              color: Colors.white.withValues(alpha: 0.16),
              borderRadius: BorderRadius.circular(999),
              border: Border.all(color: Colors.white.withValues(alpha: 0.12)),
            ),
            child: Text(
              '$totalCount aktif plaj',
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.w700,
                  ),
            ),
          ),
          const Gap(18),
          child,
        ],
      ),
    );
  }
}

class _StatusBadge extends StatelessWidget {
  const _StatusBadge({
    required this.label,
    required this.color,
  });

  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: color.withAlpha(220),
        border: Border.all(color: color.withValues(alpha: 0.35)),
        borderRadius: BorderRadius.circular(999),
      ),
      child: Text(
        label,
        style: const TextStyle(
          color: Colors.white,
          fontSize: 11,
          fontWeight: FontWeight.w800,
        ),
      ),
    );
  }
}

class _CardFallback extends StatelessWidget {
  const _CardFallback(this.beach);

  final Beach beach;

  @override
  Widget build(BuildContext context) {
    final initial = beach.name.isNotEmpty ? beach.name[0] : 'B';

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
          initial.toUpperCase(),
          style: const TextStyle(
            color: Colors.white,
            fontSize: 56,
            fontWeight: FontWeight.w900,
          ),
        ),
      ),
    );
  }
}

const _facilityMeta = <String, (IconData, Color)>{
  'hasBar': (Icons.local_bar_rounded, Colors.deepPurple),
  'hasPool': (Icons.pool_rounded, Colors.blue),
  'hasWifi': (Icons.wifi_rounded, Colors.teal),
  'hasParking': (Icons.local_parking_rounded, Colors.blueGrey),
  'hasRestaurant': (Icons.restaurant_rounded, Colors.deepOrange),
  'hasWaterSports': (Icons.kayaking_rounded, Colors.cyan),
  'isChildFriendly': (Icons.child_care_rounded, Colors.pink),
  'hasSunbeds': (Icons.deck_rounded, Colors.amber),
  'hasShower': (Icons.shower_rounded, Colors.lightBlue),
  'hasDJ': (Icons.music_note_rounded, Colors.indigo),
};

String _friendlyErrorMessage(String message) {
  final normalized = message.toLowerCase();

  if (normalized.contains('connection refused') ||
      normalized.contains('socketexception') ||
      normalized.contains('failed host lookup')) {
    return 'Sunucuya su an ulasilamiyor. Baglantinizi kontrol edip tekrar deneyin.';
  }

  if (normalized.contains('timeout')) {
    return 'Istek zaman asimina ugradi. Birazdan yeniden deneyin.';
  }

  return message;
}
