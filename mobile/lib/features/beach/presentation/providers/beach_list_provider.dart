import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/models/models.dart';
import 'package:beachgo/features/beach/data/beach_repository.dart';

final beachListControllerProvider =
    AsyncNotifierProvider<BeachListController, BeachListState>(
  BeachListController.new,
);

final beachDetailProvider = FutureProvider.family<BeachDto?, int>((ref, id) {
  return ref.watch(beachRepositoryProvider).getBeachById(id);
});

class BeachListController extends AsyncNotifier<BeachListState> {
  @override
  Future<BeachListState> build() async {
    return _loadInitial();
  }

  Future<void> reload() async {
    final previous = state.valueOrNull;
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      if (previous == null) return _loadInitial();

      if (previous.query.isNotEmpty) {
        final result = await ref
            .read(beachRepositoryProvider)
            .searchBeaches(previous.query);
        return previous.copyWith(
          beaches: result.beaches,
          isUsingMockData: result.isMock,
        );
      }

      if (previous.hasActiveFilters) {
        final result = await ref
            .read(beachRepositoryProvider)
            .filterBeaches(previous.filters);
        return previous.copyWith(
          beaches: result.beaches,
          isUsingMockData: result.isMock,
        );
      }

      return _loadInitial();
    });
  }

  Future<void> search(String query) async {
    final current = state.valueOrNull ?? BeachListState.initial();
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      final trimmed = query.trim();
      if (trimmed.isEmpty) {
        final result = await ref.read(beachRepositoryProvider).getBeaches();
        return current.copyWith(
          beaches: result.beaches,
          query: '',
          filters: BeachListState.defaultFilter,
          clearActiveCategory: true,
          isUsingMockData: result.isMock,
        );
      }

      final result = await ref.read(beachRepositoryProvider).searchBeaches(trimmed);
      return current.copyWith(
        beaches: result.beaches,
        query: trimmed,
        filters: BeachListState.defaultFilter,
        clearActiveCategory: true,
        isUsingMockData: result.isMock,
      );
    });
  }

  Future<void> applyFilters(
    BeachFilter filters, {
    String? activeCategory,
  }) async {
    final current = state.valueOrNull ?? BeachListState.initial();
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      final result = await ref.read(beachRepositoryProvider).filterBeaches(filters);
      return current.copyWith(
        beaches: result.beaches,
        query: '',
        filters: filters,
        activeCategory: activeCategory,
        clearActiveCategory: activeCategory == null,
        isUsingMockData: result.isMock,
      );
    });
  }

  Future<void> clearFilters() async {
    final current = state.valueOrNull ?? BeachListState.initial();
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      final result = await ref.read(beachRepositoryProvider).getBeaches();
      return current.copyWith(
        beaches: result.beaches,
        query: '',
        filters: BeachListState.defaultFilter,
        clearActiveCategory: true,
        isUsingMockData: result.isMock,
      );
    });
  }

  Future<BeachListState> _loadInitial() async {
    final result = await ref.read(beachRepositoryProvider).getBeaches();
    return BeachListState(
      beaches: result.beaches,
      query: '',
      filters: BeachListState.defaultFilter,
      activeCategory: null,
      isUsingMockData: result.isMock,
    );
  }
}

class BeachListState {
  const BeachListState({
    required this.beaches,
    required this.query,
    required this.filters,
    required this.activeCategory,
    required this.isUsingMockData,
  });

  static const BeachFilter defaultFilter = BeachFilter(sortBy: 'rating');

  factory BeachListState.initial() {
    return const BeachListState(
      beaches: [],
      query: '',
      filters: defaultFilter,
      activeCategory: null,
      isUsingMockData: false,
    );
  }

  final List<BeachDto> beaches;
  final String query;
  final BeachFilter filters;
  final String? activeCategory;
  final bool isUsingMockData;

  bool get hasActiveFilters =>
      filters.minRating != null ||
      filters.hasBar == true ||
      filters.hasWaterSports == true ||
      filters.isChildFriendly == true ||
      filters.hasPool == true ||
      filters.freeEntry == true ||
      filters.sortBy == 'occupancy';

  int get activeFilterCount {
    var count = 0;
    if (filters.minRating != null) count++;
    if (filters.hasBar == true) count++;
    if (filters.hasWaterSports == true) count++;
    if (filters.isChildFriendly == true) count++;
    if (filters.hasPool == true) count++;
    if (filters.freeEntry == true) count++;
    return count;
  }

  BeachListState copyWith({
    List<BeachDto>? beaches,
    String? query,
    BeachFilter? filters,
    String? activeCategory,
    bool clearActiveCategory = false,
    bool? isUsingMockData,
  }) {
    return BeachListState(
      beaches: beaches ?? this.beaches,
      query: query ?? this.query,
      filters: filters ?? this.filters,
      activeCategory:
          clearActiveCategory ? null : (activeCategory ?? this.activeCategory),
      isUsingMockData: isUsingMockData ?? this.isUsingMockData,
    );
  }
}
