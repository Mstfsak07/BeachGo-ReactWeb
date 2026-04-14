import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/core/network/paged_response.dart';
import 'package:beachgo/core/pagination/paginated_notifier.dart';
import 'package:beachgo/core/pagination/paginated_result.dart';
import 'package:beachgo/core/pagination/paginated_state.dart';
import 'package:beachgo/features/beach/data/repository/beach_repository.dart';
import 'package:beachgo/features/beach/domain/entities/beach.dart';
import 'package:beachgo/features/beach/domain/entities/beach_filter.dart';

final beachListControllerProvider =
    NotifierProvider<BeachListNotifier, BeachListState>(
  BeachListNotifier.new,
);

final beachDetailProvider = FutureProvider.family<Result<Beach>, int>((ref, id) {
  return ref.watch(beachRepositoryProvider).getBeachById(id);
});

class BeachListNotifier extends PaginatedNotifier<Beach, BeachListState> {
  static const int _defaultPageSize = 10;

  @override
  BeachListState createInitialState() {
    return BeachListState.initial(pageSize: _defaultPageSize);
  }

  @override
  Object itemIdentity(Beach item) => item.id;

  @override
  BeachListState resetStateForInitialLoad(BeachListState current) {
    return current
        .copyWith(
          query: '',
          filters: BeachListState.defaultFilter,
          clearActiveCategory: true,
        )
        .copyWithPagination(
          items: const <Beach>[],
          isInitialLoading: true,
          isLoadingMore: false,
          isRefreshing: false,
          hasMore: true,
          currentPage: 0,
          totalCount: 0,
          error: null,
        );
  }

  @override
  bool canLoadMore(BeachListState current) {
    return current.isPaginatedMode && super.canLoadMore(current);
  }

  @override
  Future<PaginatedResult<Beach>> fetchPage(int page, int pageSize) async {
    final result = await ref.read(beachRepositoryProvider).getBeaches(
          page: page,
          pageSize: pageSize,
        );

    return switch (result) {
      Success<PagedResponse<Beach>>(data: final pageData) => PaginatedResult<Beach>(
          items: pageData.items,
          page: pageData.page,
          pageSize: pageData.pageSize,
          totalCount: pageData.totalCount,
        ),
      FailureResult<PagedResponse<Beach>>(failure: final failure) => throw failure,
    };
  }

  Future<void> search(String query) async {
    if (state.isInitialLoading || state.isRefreshing) {
      return;
    }

    final trimmed = query.trim();
    if (trimmed.isEmpty) {
      await loadInitial();
      return;
    }

    final previous = state;
    state = state
        .copyWith(
          query: trimmed,
          filters: BeachListState.defaultFilter,
          clearActiveCategory: true,
        )
        .copyWithPagination(
          isInitialLoading: true,
          isLoadingMore: false,
          isRefreshing: false,
          error: null,
        );

    final result = await ref.read(beachRepositoryProvider).searchBeaches(trimmed);
    state = _mapListResult(
      result,
      previous: previous,
      query: trimmed,
      filters: BeachListState.defaultFilter,
      clearActiveCategory: true,
      isInitialLoading: false,
    );
  }

  Future<void> applyFilters(
    BeachFilter filters, {
    String? activeCategory,
  }) async {
    if (state.isInitialLoading || state.isRefreshing) {
      return;
    }

    final previous = state;
    state = state
        .copyWith(
          query: '',
          filters: filters,
          activeCategory: activeCategory,
          clearActiveCategory: activeCategory == null,
        )
        .copyWithPagination(
          isInitialLoading: true,
          isLoadingMore: false,
          isRefreshing: false,
          error: null,
        );

    final result = await ref.read(beachRepositoryProvider).filterBeaches(filters);
    state = _mapListResult(
      result,
      previous: previous,
      query: '',
      filters: filters,
      activeCategory: activeCategory,
      clearActiveCategory: activeCategory == null,
      isInitialLoading: false,
    );
  }

  Future<void> clearFilters() async {
    await loadInitial();
  }

  @override
  Future<void> refresh() async {
    if (state.query.isNotEmpty || state.hasActiveFilters) {
      await _refreshFilteredState();
      return;
    }

    await super.refresh();
  }

  Future<void> _refreshFilteredState() async {
    if (state.isInitialLoading || state.isRefreshing) {
      return;
    }

    final previous = state;
    state = state.copyWithPagination(
      isRefreshing: true,
      isLoadingMore: false,
      error: null,
    );

    if (previous.query.isNotEmpty) {
      final result =
          await ref.read(beachRepositoryProvider).searchBeaches(previous.query);
      state = _mapListResult(
        result,
        previous: previous,
        query: previous.query,
        filters: previous.filters,
        activeCategory: previous.activeCategory,
        isRefreshing: false,
      );
      return;
    }

    final result =
        await ref.read(beachRepositoryProvider).filterBeaches(previous.filters);
    state = _mapListResult(
      result,
      previous: previous,
      filters: previous.filters,
      activeCategory: previous.activeCategory,
      isRefreshing: false,
    );
  }

  BeachListState _mapListResult(
    Result<List<Beach>> result, {
    required BeachListState previous,
    String? query,
    BeachFilter? filters,
    String? activeCategory,
    bool clearActiveCategory = false,
    bool isInitialLoading = false,
    bool isRefreshing = false,
  }) {
    return switch (result) {
      Success<List<Beach>>(data: final beaches) => previous
          .copyWith(
            query: query ?? previous.query,
            filters: filters ?? previous.filters,
            activeCategory: activeCategory,
            clearActiveCategory: clearActiveCategory,
          )
          .copyWithPagination(
            items: List<Beach>.unmodifiable(beaches),
            currentPage: beaches.isEmpty ? 0 : 1,
            totalCount: beaches.length,
            hasMore: false,
            isInitialLoading: isInitialLoading,
            isRefreshing: isRefreshing,
            isLoadingMore: false,
            error: null,
          ),
      FailureResult<List<Beach>>(failure: final failure) => previous
          .copyWith(
            query: query ?? previous.query,
            filters: filters ?? previous.filters,
            activeCategory: activeCategory,
            clearActiveCategory: clearActiveCategory,
          )
          .copyWithPagination(
            isInitialLoading: false,
            isRefreshing: false,
            isLoadingMore: false,
            error: failure,
          ),
    };
  }
}

class BeachListState extends PaginatedState<Beach, BeachListState> {
  const BeachListState({
    required super.items,
    required super.isInitialLoading,
    required super.isLoadingMore,
    required super.isRefreshing,
    required super.hasMore,
    required super.currentPage,
    required super.pageSize,
    required super.totalCount,
    required super.error,
    required this.query,
    required this.filters,
    required this.activeCategory,
  });

  static const BeachFilter defaultFilter = BeachFilter(sortBy: 'rating');

  factory BeachListState.initial({int pageSize = 10}) {
    return BeachListState(
      items: const <Beach>[],
      isInitialLoading: false,
      isLoadingMore: false,
      isRefreshing: false,
      hasMore: true,
      currentPage: 0,
      pageSize: pageSize,
      totalCount: 0,
      error: null,
      query: '',
      filters: defaultFilter,
      activeCategory: null,
    );
  }

  final String query;
  final BeachFilter filters;
  final String? activeCategory;

  bool get hasActiveFilters =>
      filters.hasWifi == true ||
      filters.minRating != null ||
      filters.hasBar == true ||
      filters.hasWaterSports == true ||
      filters.isChildFriendly == true ||
      filters.hasPool == true ||
      filters.freeEntry == true ||
      filters.sortBy == 'occupancy';

  int get activeFilterCount {
    var count = 0;
    if (filters.hasWifi == true) count++;
    if (filters.minRating != null) count++;
    if (filters.hasBar == true) count++;
    if (filters.hasWaterSports == true) count++;
    if (filters.isChildFriendly == true) count++;
    if (filters.hasPool == true) count++;
    if (filters.freeEntry == true) count++;
    return count;
  }

  bool get isPaginatedMode => query.isEmpty && !hasActiveFilters;

  BeachListState copyWith({
    String? query,
    BeachFilter? filters,
    String? activeCategory,
    bool clearActiveCategory = false,
  }) {
    return BeachListState(
      items: items,
      isInitialLoading: isInitialLoading,
      isLoadingMore: isLoadingMore,
      isRefreshing: isRefreshing,
      hasMore: hasMore,
      currentPage: currentPage,
      pageSize: pageSize,
      totalCount: totalCount,
      error: error,
      query: query ?? this.query,
      filters: filters ?? this.filters,
      activeCategory:
          clearActiveCategory ? null : (activeCategory ?? this.activeCategory),
    );
  }

  @override
  BeachListState copyWithPagination({
    List<Beach>? items,
    bool? isInitialLoading,
    bool? isLoadingMore,
    bool? isRefreshing,
    bool? hasMore,
    int? currentPage,
    int? pageSize,
    int? totalCount,
    Object? error = paginationNoFailure,
    bool clearError = false,
  }) {
    return BeachListState(
      items: items ?? this.items,
      isInitialLoading: isInitialLoading ?? this.isInitialLoading,
      isLoadingMore: isLoadingMore ?? this.isLoadingMore,
      isRefreshing: isRefreshing ?? this.isRefreshing,
      hasMore: hasMore ?? this.hasMore,
      currentPage: currentPage ?? this.currentPage,
      pageSize: pageSize ?? this.pageSize,
      totalCount: totalCount ?? this.totalCount,
      error: clearError
          ? null
          : identical(error, paginationNoFailure)
              ? this.error
              : error as Failure?,
      query: query,
      filters: filters,
      activeCategory: activeCategory,
    );
  }
}
