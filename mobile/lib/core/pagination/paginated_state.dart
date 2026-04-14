import 'package:beachgo/core/error/failures.dart';

const paginationNoFailure = Object();

abstract class PaginatedState<T, S extends PaginatedState<T, S>> {
  const PaginatedState({
    required this.items,
    required this.isInitialLoading,
    required this.isLoadingMore,
    required this.isRefreshing,
    required this.hasMore,
    required this.currentPage,
    required this.pageSize,
    required this.totalCount,
    required this.error,
  });

  final List<T> items;
  final bool isInitialLoading;
  final bool isLoadingMore;
  final bool isRefreshing;
  final bool hasMore;
  final int currentPage;
  final int pageSize;
  final int totalCount;
  final Failure? error;

  String? get errorMessage => error?.message;
  bool get showInitialError => error != null && items.isEmpty && !isInitialLoading;
  bool get showEmptyState => error == null && items.isEmpty && !isInitialLoading;

  S copyWithPagination({
    List<T>? items,
    bool? isInitialLoading,
    bool? isLoadingMore,
    bool? isRefreshing,
    bool? hasMore,
    int? currentPage,
    int? pageSize,
    int? totalCount,
    Object? error = paginationNoFailure,
    bool clearError = false,
  });
}
