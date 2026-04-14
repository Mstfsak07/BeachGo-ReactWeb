import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/pagination/paginated_notifier.dart';
import 'package:beachgo/core/pagination/paginated_result.dart';
import 'package:beachgo/core/pagination/paginated_state.dart';

void main() {
  test('generic notifier appends pages and computes hasMore', () async {
    final repository = _FakeFavoriteRepository(
      loader: ({required int page, required int pageSize}) async {
        if (page == 1) {
          return PaginatedResult<_Favorite>(
            items: const [_Favorite(id: 1, name: 'A'), _Favorite(id: 2, name: 'B')],
            page: 1,
            pageSize: pageSize,
            totalCount: 3,
          );
        }

        return PaginatedResult<_Favorite>(
          items: const [_Favorite(id: 3, name: 'C')],
          page: 2,
          pageSize: pageSize,
          totalCount: 3,
        );
      },
    );

    final container = ProviderContainer(
      overrides: [
        _favoriteRepositoryProvider.overrideWithValue(repository),
      ],
    );
    addTearDown(container.dispose);

    container.read(_favoriteListProvider);
    await _waitForFavoriteState(container);

    var state = container.read(_favoriteListProvider);
    expect(state.items.map((item) => item.id), [1, 2]);
    expect(state.hasMore, isTrue);

    await container.read(_favoriteListProvider.notifier).loadMore();

    state = container.read(_favoriteListProvider);
    expect(state.items.map((item) => item.id), [1, 2, 3]);
    expect(state.currentPage, 2);
    expect(state.hasMore, isFalse);
  });

  test('generic notifier keeps items on refresh failure', () async {
    var shouldFail = false;
    final repository = _FakeFavoriteRepository(
      loader: ({required int page, required int pageSize}) async {
        if (shouldFail) {
          throw const ServerFailure('favorites refresh failed');
        }

        return PaginatedResult<_Favorite>(
          items: const [_Favorite(id: 1, name: 'A')],
          page: page,
          pageSize: pageSize,
          totalCount: 1,
        );
      },
    );

    final container = ProviderContainer(
      overrides: [
        _favoriteRepositoryProvider.overrideWithValue(repository),
      ],
    );
    addTearDown(container.dispose);

    container.read(_favoriteListProvider);
    await _waitForFavoriteState(container);

    shouldFail = true;
    await container.read(_favoriteListProvider.notifier).refresh();

    final state = container.read(_favoriteListProvider);
    expect(state.items.map((item) => item.id), [1]);
    expect(state.errorMessage, 'favorites refresh failed');
    expect(state.isRefreshing, isFalse);
  });
}

final _favoriteRepositoryProvider = Provider<_FakeFavoriteRepository>((ref) {
  throw UnimplementedError();
});

final _favoriteListProvider =
    NotifierProvider<_FavoriteListNotifier, _FavoriteListState>(
  _FavoriteListNotifier.new,
);

class _FavoriteListNotifier
    extends PaginatedNotifier<_Favorite, _FavoriteListState> {
  @override
  _FavoriteListState createInitialState() {
    return const _FavoriteListState(
      items: <_Favorite>[],
      isInitialLoading: false,
      isLoadingMore: false,
      isRefreshing: false,
      hasMore: true,
      currentPage: 0,
      pageSize: 10,
      totalCount: 0,
      error: null,
    );
  }

  @override
  Object itemIdentity(_Favorite item) => item.id;

  @override
  Future<PaginatedResult<_Favorite>> fetchPage(int page, int pageSize) {
    return ref.read(_favoriteRepositoryProvider).getFavoritesPage(
          page: page,
          pageSize: pageSize,
        );
  }
}

class _FavoriteListState
    extends PaginatedState<_Favorite, _FavoriteListState> {
  const _FavoriteListState({
    required super.items,
    required super.isInitialLoading,
    required super.isLoadingMore,
    required super.isRefreshing,
    required super.hasMore,
    required super.currentPage,
    required super.pageSize,
    required super.totalCount,
    required super.error,
  });

  @override
  _FavoriteListState copyWithPagination({
    List<_Favorite>? items,
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
    return _FavoriteListState(
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
    );
  }
}

class _FakeFavoriteRepository {
  const _FakeFavoriteRepository({
    required this.loader,
  });

  final Future<PaginatedResult<_Favorite>> Function({
    required int page,
    required int pageSize,
  }) loader;

  Future<PaginatedResult<_Favorite>> getFavoritesPage({
    required int page,
    required int pageSize,
  }) {
    return loader(page: page, pageSize: pageSize);
  }
}

class _Favorite {
  const _Favorite({
    required this.id,
    required this.name,
  });

  final int id;
  final String name;
}

Future<void> _waitForFavoriteState(ProviderContainer container) async {
  await Future<void>.delayed(Duration.zero);
  for (var i = 0; i < 20; i++) {
    final state = container.read(_favoriteListProvider);
    if (!state.isInitialLoading && !state.isRefreshing && !state.isLoadingMore) {
      return;
    }
    await Future<void>.delayed(const Duration(milliseconds: 10));
  }
}
