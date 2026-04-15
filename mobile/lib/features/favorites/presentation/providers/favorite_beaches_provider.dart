import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/features/beach/data/repository/beach_repository.dart';
import 'package:beachgo/features/beach/domain/entities/beach.dart';

final favoriteBeachesProvider =
    AsyncNotifierProvider<FavoriteBeachesNotifier, List<Beach>>(
  FavoriteBeachesNotifier.new,
);

class FavoriteBeachesNotifier extends AsyncNotifier<List<Beach>> {
  @override
  Future<List<Beach>> build() async {
    final result = await ref.read(beachRepositoryProvider).getFavoriteBeaches();
    return switch (result) {
      Success<List<Beach>>(data: final beaches) => beaches,
      FailureResult<List<Beach>>(failure: final failure) => throw failure,
    };
  }

  Future<void> refresh() async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(build);
  }

  Future<Failure?> toggleFavorite(Beach beach) async {
    final current = state.valueOrNull ?? const <Beach>[];
    final exists = current.any((item) => item.id == beach.id);

    final optimistic = exists
        ? current.where((item) => item.id != beach.id).toList(growable: false)
        : <Beach>[beach, ...current];
    state = AsyncData(optimistic);

    final repository = ref.read(beachRepositoryProvider);
    final result = exists
        ? await repository.removeFavorite(beach.id)
        : await repository.addFavorite(beach.id);

    return switch (result) {
      Success<void>() => null,
      FailureResult<void>(failure: final failure) => () {
          state = AsyncData(current);
          return failure;
        }(),
    };
  }

  bool isFavorite(int beachId) {
    return state.valueOrNull?.any((item) => item.id == beachId) ?? false;
  }
}
