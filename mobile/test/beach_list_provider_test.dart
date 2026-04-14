import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/core/network/paged_response.dart';
import 'package:beachgo/features/beach/data/repository/beach_repository.dart';
import 'package:beachgo/features/beach/domain/entities/beach.dart';
import 'package:beachgo/features/beach/domain/entities/beach_filter.dart';
import 'package:beachgo/features/beach/domain/entities/weather.dart';
import 'package:beachgo/features/beach/presentation/providers/beach_list_provider.dart';

void main() {
  test('loadInitial and loadMore append items and stop at last page', () async {
    final repository = _ControlledBeachRepository(
      pagedLoader: ({required int page, required int pageSize}) async {
        if (page == 1) {
          return Success(
            PagedResponse(
              items: [_beach(1), _beach(2)],
              page: 1,
              pageSize: pageSize,
              totalCount: 3,
            ),
          );
        }

        return Success(
          PagedResponse(
            items: [_beach(3)],
            page: 2,
            pageSize: pageSize,
            totalCount: 3,
          ),
        );
      },
    );

    final container = ProviderContainer(
      overrides: [
        beachRepositoryProvider.overrideWithValue(repository),
      ],
    );
    addTearDown(container.dispose);

    container.read(beachListControllerProvider);
    await _waitForSettledState(container);

    var state = container.read(beachListControllerProvider);
    expect(state.items.map((item) => item.id), [1, 2]);
    expect(state.currentPage, 1);
    expect(state.hasMore, isTrue);

    await container.read(beachListControllerProvider.notifier).loadMore();

    state = container.read(beachListControllerProvider);
    expect(state.items.map((item) => item.id), [1, 2, 3]);
    expect(state.currentPage, 2);
    expect(state.hasMore, isFalse);
  });

  test('loadMore prevents duplicate requests while a page is already loading', () async {
    final completer = Completer<Result<PagedResponse<Beach>>>();
    final requestedPages = <int>[];

    final repository = _ControlledBeachRepository(
      pagedLoader: ({required int page, required int pageSize}) async {
        requestedPages.add(page);
        if (page == 1) {
          return Success(
            PagedResponse(
              items: [_beach(1), _beach(2)],
              page: 1,
              pageSize: pageSize,
              totalCount: 4,
            ),
          );
        }

        return completer.future;
      },
    );

    final container = ProviderContainer(
      overrides: [
        beachRepositoryProvider.overrideWithValue(repository),
      ],
    );
    addTearDown(container.dispose);

    container.read(beachListControllerProvider);
    await _waitForSettledState(container);

    final notifier = container.read(beachListControllerProvider.notifier);
    final firstLoadMore = notifier.loadMore();
    final secondLoadMore = notifier.loadMore();

    expect(requestedPages.where((page) => page == 2).length, 1);

    completer.complete(
      Success(
        PagedResponse(
          items: [_beach(3), _beach(4)],
          page: 2,
          pageSize: 10,
          totalCount: 4,
        ),
      ),
    );

    await Future.wait([firstLoadMore, secondLoadMore]);

    final state = container.read(beachListControllerProvider);
    expect(state.items.map((item) => item.id), [1, 2, 3, 4]);
    expect(state.hasMore, isFalse);
  });

  test('refresh failure keeps existing items visible', () async {
    var refreshShouldFail = false;

    final repository = _ControlledBeachRepository(
      pagedLoader: ({required int page, required int pageSize}) async {
        if (refreshShouldFail) {
          return const FailureResult(ServerFailure('refresh failed'));
        }

        return Success(
          PagedResponse(
            items: [_beach(1), _beach(2)],
            page: 1,
            pageSize: pageSize,
            totalCount: 2,
          ),
        );
      },
    );

    final container = ProviderContainer(
      overrides: [
        beachRepositoryProvider.overrideWithValue(repository),
      ],
    );
    addTearDown(container.dispose);

    container.read(beachListControllerProvider);
    await _waitForSettledState(container);

    refreshShouldFail = true;
    await container.read(beachListControllerProvider.notifier).refresh();

    final state = container.read(beachListControllerProvider);
    expect(state.items.map((item) => item.id), [1, 2]);
    expect(state.errorMessage, 'refresh failed');
    expect(state.isRefreshing, isFalse);
  });
}

Future<void> _waitForSettledState(ProviderContainer container) async {
  await Future<void>.delayed(Duration.zero);
  for (var i = 0; i < 20; i++) {
    final state = container.read(beachListControllerProvider);
    if (!state.isInitialLoading && !state.isRefreshing && !state.isLoadingMore) {
      return;
    }
    await Future<void>.delayed(const Duration(milliseconds: 10));
  }
}

class _ControlledBeachRepository extends BeachRepository {
  _ControlledBeachRepository({
    required this.pagedLoader,
  });

  final Future<Result<PagedResponse<Beach>>> Function({
    required int page,
    required int pageSize,
  }) pagedLoader;

  @override
  Future<Result<PagedResponse<Beach>>> getBeaches({
    int page = 1,
    int pageSize = 20,
  }) {
    return pagedLoader(page: page, pageSize: pageSize);
  }

  @override
  Future<Result<List<Beach>>> searchBeaches(String query) async {
    final response = await getBeaches();
    return switch (response) {
      Success<PagedResponse<Beach>>(data: final data) => Success(data.items),
      FailureResult<PagedResponse<Beach>>(failure: final failure) =>
        FailureResult(failure),
    };
  }

  @override
  Future<Result<List<Beach>>> filterBeaches(BeachFilter filter) {
    return searchBeaches('');
  }

  @override
  Future<Result<Beach>> getBeachById(int id) async {
    return Success(_beach(id));
  }

  @override
  Future<Result<Weather>> getBeachWeather(int id) async {
    return const Success(
      Weather(
        temperature: null,
        description: '',
        windSpeed: null,
        seaTemperature: null,
        waveHeight: null,
      ),
    );
  }
}

Beach _beach(int id) {
  return Beach(
    id: id,
    name: 'Beach $id',
    location: '',
    address: 'Antalya',
    imageUrl: '',
    entryFee: 0,
    rating: 4.5,
    reviewCount: 0,
    occupancyPercent: 0,
    openTime: '09:00',
    closeTime: '18:00',
    capacity: 0,
    facilities: const [],
    latitude: null,
    longitude: null,
    description: '',
    hasEntryFee: false,
    isOpen: true,
    sunbedPrice: 0,
    phone: '',
    website: '',
    instagram: '',
    hasBar: false,
    hasWaterSports: false,
    isChildFriendly: false,
    hasPool: false,
    hasRestaurant: false,
    hasWifi: false,
    hasParking: false,
    hasSunbeds: false,
    hasShower: false,
    hasDJ: false,
  );
}
