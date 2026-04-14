import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/pagination/paginated_result.dart';
import 'package:beachgo/core/pagination/paginated_state.dart';

abstract class PaginatedNotifier<T, S extends PaginatedState<T, S>>
    extends Notifier<S> {
  @override
  S build() {
    Future<void>.microtask(loadInitial);
    return createInitialState();
  }

  S createInitialState();

  Future<PaginatedResult<T>> fetchPage(int page, int pageSize);

  Object itemIdentity(T item) => item as Object;

  S resetStateForInitialLoad(S current) {
    return current.copyWithPagination(
      items: List<T>.empty(growable: false),
      isInitialLoading: true,
      isLoadingMore: false,
      isRefreshing: false,
      hasMore: true,
      currentPage: 0,
      totalCount: 0,
      error: null,
    );
  }

  Future<void> loadInitial() async {
    if (state.isInitialLoading) {
      return;
    }

    state = resetStateForInitialLoad(state);

    try {
      final page = await fetchPage(1, state.pageSize);
      state = applyPage(
        state,
        page,
        append: false,
        isInitialLoading: false,
        isRefreshing: false,
        isLoadingMore: false,
      );
    } on Failure catch (failure) {
      state = state.copyWithPagination(
        items: List<T>.empty(growable: false),
        isInitialLoading: false,
        isLoadingMore: false,
        isRefreshing: false,
        hasMore: false,
        currentPage: 0,
        totalCount: 0,
        error: failure,
      );
    }
  }

  Future<void> loadMore() async {
    if (!canLoadMore(state)) {
      return;
    }

    final previousItems = state.items;
    state = state.copyWithPagination(
      isLoadingMore: true,
      error: null,
    );

    try {
      final page = await fetchPage(state.currentPage + 1, state.pageSize);
      state = applyPage(
        state.copyWithPagination(items: previousItems),
        page,
        append: true,
        isInitialLoading: false,
        isRefreshing: false,
        isLoadingMore: false,
      );
    } on Failure catch (failure) {
      state = state.copyWithPagination(
        items: previousItems,
        isLoadingMore: false,
        error: failure,
      );
    }
  }

  Future<void> refresh() async {
    if (state.isInitialLoading || state.isRefreshing) {
      return;
    }

    final previous = state;
    state = state.copyWithPagination(
      isRefreshing: true,
      isLoadingMore: false,
      error: null,
    );

    try {
      final page = await fetchPage(1, previous.pageSize);
      state = applyPage(
        previous,
        page,
        append: false,
        isInitialLoading: false,
        isRefreshing: false,
        isLoadingMore: false,
      );
    } on Failure catch (failure) {
      state = previous.copyWithPagination(
        isRefreshing: false,
        error: failure,
      );
    }
  }

  bool canLoadMore(S current) {
    return !current.isInitialLoading &&
        !current.isLoadingMore &&
        !current.isRefreshing &&
        current.hasMore;
  }

  S applyPage(
    S current,
    PaginatedResult<T> page, {
    required bool append,
    required bool isInitialLoading,
    required bool isRefreshing,
    required bool isLoadingMore,
  }) {
    final items = append
        ? _mergeItems(current.items, page.items)
        : List<T>.unmodifiable(page.items);

    return current.copyWithPagination(
      items: items,
      currentPage: page.page,
      pageSize: page.pageSize,
      totalCount: page.totalCount,
      hasMore: items.length < page.totalCount && page.items.isNotEmpty,
      isInitialLoading: isInitialLoading,
      isRefreshing: isRefreshing,
      isLoadingMore: isLoadingMore,
      error: null,
    );
  }

  List<T> _mergeItems(List<T> existing, List<T> incoming) {
    final seen = existing.map(itemIdentity).toSet();
    final merged = <T>[...existing];

    for (final item in incoming) {
      if (seen.add(itemIdentity(item))) {
        merged.add(item);
      }
    }

    return List<T>.unmodifiable(merged);
  }
}
