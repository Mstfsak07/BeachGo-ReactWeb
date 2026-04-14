import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:gap/gap.dart';
import 'package:go_router/go_router.dart';

import 'package:beachgo/core/models/models.dart';
import 'package:beachgo/core/theme/app_theme.dart';
import 'package:beachgo/features/beach/presentation/providers/beach_list_provider.dart';
import 'package:beachgo/shared/widgets/shared_widgets.dart';

class BeachListScreen extends ConsumerStatefulWidget {
  const BeachListScreen({super.key});

  @override
  ConsumerState<BeachListScreen> createState() => _BeachListScreenState();
}

class _BeachListScreenState extends ConsumerState<BeachListScreen> {
  late final TextEditingController _searchController;

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
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final beachStateAsync = ref.watch(beachListControllerProvider);

    ref.listen<AsyncValue<BeachListState>>(beachListControllerProvider, (_, next) {
      final nextValue = next.valueOrNull;
      if (nextValue != null && _searchController.text != nextValue.query) {
        _searchController.text = nextValue.query;
      }
    });

    return Scaffold(
      body: SafeArea(
        child: beachStateAsync.when(
          data: (state) => RefreshIndicator(
            onRefresh: () => ref.read(beachListControllerProvider.notifier).reload(),
            child: CustomScrollView(
              physics: const AlwaysScrollableScrollPhysics(),
              slivers: [
                SliverToBoxAdapter(
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(20, 20, 20, 12),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Plajlari Kesfet',
                          style: Theme.of(context).textTheme.headlineLarge,
                        ),
                        const Gap(8),
                        Text(
                          'React tarafindaki arama, filtre ve kart akisini mobile-first bir liste deneyimine tasidim.',
                          style: Theme.of(context).textTheme.bodyLarge,
                        ),
                        const Gap(16),
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
                        const Gap(16),
                        SizedBox(
                          height: 40,
                          child: ListView.separated(
                            scrollDirection: Axis.horizontal,
                            itemBuilder: (context, index) {
                              final entry = _categoryFilters.entries.elementAt(index);
                              final isActive = state.activeCategory == entry.key;
                              return ChoiceChip(
                                label: Text(entry.key),
                                selected: isActive,
                                onSelected: (_) {
                                  ref
                                      .read(beachListControllerProvider.notifier)
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
                        if (state.isUsingMockData) ...[
                          const Gap(14),
                          Container(
                            padding: const EdgeInsets.all(12),
                            decoration: BoxDecoration(
                              color: AppColors.primaryLight,
                              borderRadius: BorderRadius.circular(14),
                            ),
                            child: Row(
                              children: [
                                const Icon(
                                  Icons.info_outline,
                                  color: AppColors.primary,
                                ),
                                const Gap(10),
                                Expanded(
                                  child: Text(
                                    'API erisilemedigi icin ekran su an yerel mock veri ile calisiyor. Repository gercek servise de bagli.',
                                    style: Theme.of(context).textTheme.bodyMedium,
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ],
                        const Gap(12),
                        Text(
                          '${state.beaches.length} plaj listeleniyor',
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                      ],
                    ),
                  ),
                ),
                if (state.beaches.isEmpty)
                  const SliverFillRemaining(
                    hasScrollBody: false,
                    child: Center(
                      child: AppErrorWidget(
                        message:
                            'Bu kriterlerle eslesen plaj bulunamadi. Aramayi veya filtreleri temizleyip tekrar deneyin.',
                      ),
                    ),
                  )
                else
                  SliverPadding(
                    padding: const EdgeInsets.fromLTRB(20, 4, 20, 24),
                    sliver: SliverList.separated(
                      itemCount: state.beaches.length,
                      separatorBuilder: (_, __) => const SizedBox(height: 16),
                      itemBuilder: (context, index) {
                        final beach = state.beaches[index];
                        return _BeachCard(
                          beach: beach,
                          onTap: () {
                            if (beach.id != null) {
                              context.push('/beaches/${beach.id}');
                            }
                          },
                        );
                      },
                    ),
                  ),
              ],
            ),
          ),
          loading: () => ListView.separated(
            padding: const EdgeInsets.fromLTRB(20, 20, 20, 24),
            itemBuilder: (_, __) => const SizedBox(
              height: 280,
              child: BeachCardSkeleton(),
            ),
            separatorBuilder: (_, __) => const SizedBox(height: 16),
            itemCount: 4,
          ),
          error: (_, __) => Center(
            child: AppErrorWidget(
              message: 'Plajlar yuklenemedi.',
              onRetry: () => ref.read(beachListControllerProvider.notifier).reload(),
            ),
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
    return Row(
      children: [
        Expanded(
          child: TextField(
            controller: controller,
            textInputAction: TextInputAction.search,
            onSubmitted: onSubmit,
            decoration: InputDecoration(
              hintText: 'Plaj adi, konum veya aciklama ara',
              prefixIcon: const Icon(Icons.search),
              suffixIcon: controller.text.isEmpty
                  ? null
                  : IconButton(
                      onPressed: () {
                        controller.clear();
                        onClear();
                      },
                      icon: const Icon(Icons.close),
                    ),
            ),
          ),
        ),
        const Gap(12),
        Badge(
          isLabelVisible: activeFilterCount > 0,
          label: Text('$activeFilterCount'),
          child: OutlinedButton.icon(
            onPressed: onOpenFilter,
            icon: const Icon(Icons.tune_rounded),
            label: const Text('Filtrele'),
          ),
        ),
      ],
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

  final BeachDto beach;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final occupancy = beach.occupancyPercent ?? 0;
    final activeFacilities = _facilityMeta.entries
        .where((entry) => _facilityValue(beach, entry.key))
        .take(5)
        .toList();

    return InkWell(
      borderRadius: BorderRadius.circular(24),
      onTap: onTap,
      child: Ink(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(24),
          border: Border.all(color: AppColors.slate200),
          boxShadow: const [
            BoxShadow(
              color: Color(0x120F172A),
              blurRadius: 20,
              offset: Offset(0, 10),
            ),
          ],
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
                    child: beach.imageUrl != null && beach.imageUrl!.isNotEmpty
                        ? CachedNetworkImage(
                            imageUrl: beach.imageUrl!,
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
                              label: beach.hasEntryFee == true
                                  ? 'Giris ucretli'
                                  : 'Ucretsiz',
                              color: beach.hasEntryFee == true
                                  ? Colors.black87
                                  : Colors.green,
                            ),
                            const Gap(6),
                            if (beach.isOpen != null)
                              _StatusBadge(
                                label: beach.isOpen! ? 'Acik' : 'Kapali',
                                color: beach.isOpen! ? Colors.green : Colors.red,
                              ),
                          ],
                        ),
                        if ((beach.rating ?? 0) > 0)
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 10,
                              vertical: 8,
                            ),
                            decoration: BoxDecoration(
                              color: Colors.white,
                              borderRadius: BorderRadius.circular(18),
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
                                  (beach.rating ?? 0).toStringAsFixed(1),
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
                        color: Colors.black.withAlpha(90),
                        borderRadius: BorderRadius.circular(18),
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              const Text(
                                'Canli doluluk',
                                style: TextStyle(
                                  color: Colors.white,
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
                                  ? Colors.green
                                  : occupancy < 80
                                      ? Colors.orange
                                      : Colors.red,
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
                  if (beach.address != null && beach.address!.isNotEmpty)
                    Padding(
                      padding: const EdgeInsets.only(bottom: 6),
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
                              beach.address!,
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
                  Text(
                    beach.name ?? 'Adsiz Plaj',
                    style: Theme.of(context).textTheme.headlineMedium,
                  ),
                  if (beach.description != null && beach.description!.isNotEmpty)
                    Padding(
                      padding: const EdgeInsets.only(top: 8),
                      child: Text(
                        beach.description!,
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                        style: Theme.of(context).textTheme.bodyMedium,
                      ),
                    ),
                  if (activeFacilities.isNotEmpty) ...[
                    const Gap(14),
                    Wrap(
                      spacing: 10,
                      runSpacing: 10,
                      children: [
                        for (final facility in activeFacilities)
                          Icon(
                            facility.value.$1,
                            size: 18,
                            color: facility.value.$2,
                          ),
                      ],
                    ),
                  ],
                  const Gap(16),
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          '${beach.openTime ?? '--:--'} - ${beach.closeTime ?? '--:--'}',
                          style: Theme.of(context).textTheme.bodySmall,
                        ),
                      ),
                      if ((beach.reviewCount ?? 0) > 0)
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

  bool _facilityValue(BeachDto beach, String key) {
    switch (key) {
      case 'hasBar':
        return beach.hasBar == true;
      case 'hasPool':
        return beach.hasPool == true;
      case 'hasWifi':
        return beach.hasWifi == true;
      case 'hasParking':
        return beach.hasParking == true;
      case 'hasRestaurant':
        return beach.hasRestaurant == true;
      case 'hasWaterSports':
        return beach.hasWaterSports == true;
      case 'isChildFriendly':
        return beach.isChildFriendly == true;
      case 'hasSunbeds':
        return beach.hasSunbeds == true;
      case 'hasShower':
        return beach.hasShower == true;
      case 'hasDJ':
        return beach.hasDJ == true;
      default:
        return false;
    }
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

  final BeachDto beach;

  @override
  Widget build(BuildContext context) {
    final initial = (beach.name?.isNotEmpty ?? false) ? beach.name![0] : 'B';

    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          colors: [Color(0xFF2563EB), Color(0xFF38BDF8)],
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
