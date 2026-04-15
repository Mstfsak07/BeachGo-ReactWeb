import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/features/reservation/data/repositories/reservation_repository.dart';
import 'package:beachgo/features/reservation/domain/entities/reservation_confirmation.dart';
import 'package:beachgo/features/reservation/domain/entities/reservation_list_item.dart';
import 'package:beachgo/features/reservation/presentation/providers/reservation_submit_provider.dart';

void main() {
  test('reservation submit ignores duplicate submits while request is in flight', () async {
    final completer = Completer<Result<ReservationConfirmation>>();
    final repository = _FakeReservationRepository(() => completer.future);

    final container = ProviderContainer(
      overrides: [
        reservationRepositoryProvider.overrideWithValue(repository),
      ],
    );
    addTearDown(container.dispose);

    final notifier = container.read(reservationSubmitProvider(3).notifier);

    final firstCall = notifier.submit(
      beachId: 3,
      reservationDate: DateTime.utc(2026, 4, 20),
      reservationTime: '10:00',
      guestCount: 2,
      note: 'Sessiz alan rica ederim',
    );

    final secondCall = notifier.submit(
      beachId: 3,
      reservationDate: DateTime.utc(2026, 4, 20),
      reservationTime: '10:00',
      guestCount: 2,
      note: '',
    );

    expect(container.read(reservationSubmitProvider(3)).isSubmitting, isTrue);
    expect(repository.callCount, 1);
    expect(await secondCall, isNull);

    completer.complete(
      Success(
        ReservationConfirmation(
          id: 17,
          beachId: 3,
          beachName: 'Konyaalti',
          reservationDate: DateTime.utc(2026, 4, 20),
          reservationTime: '10:00',
          status: 'Pending',
        ),
      ),
    );

    final confirmation = await firstCall;
    expect(confirmation?.id, 17);
    expect(
      container.read(reservationSubmitProvider(3)).status,
      ReservationSubmitStatus.success,
    );
  });

  test('reservation submit exposes friendly failure message', () async {
    final repository = _FakeReservationRepository(
      () async => const FailureResult(
        NetworkFailure('Baglanti kurulurken sorun olustu.'),
      ),
    );

    final container = ProviderContainer(
      overrides: [
        reservationRepositoryProvider.overrideWithValue(repository),
      ],
    );
    addTearDown(container.dispose);

    final notifier = container.read(reservationSubmitProvider(8).notifier);
    final result = await notifier.submit(
      beachId: 8,
      reservationDate: DateTime.utc(2026, 5, 1),
      reservationTime: '11:30',
      guestCount: 4,
      note: '',
    );

    expect(result, isNull);
    expect(
      container.read(reservationSubmitProvider(8)).status,
      ReservationSubmitStatus.failure,
    );
    expect(
      container.read(reservationSubmitProvider(8)).errorMessage,
      'Baglanti kurulurken sorun olustu.',
    );
  });
}

class _FakeReservationRepository extends ReservationRepository {
  _FakeReservationRepository(this._handler);

  final Future<Result<ReservationConfirmation>> Function() _handler;
  int callCount = 0;

  @override
  Future<Result<ReservationConfirmation>> createReservation({
    required int beachId,
    required DateTime reservationDate,
    required String reservationTime,
    required int guestCount,
    required String note,
  }) {
    callCount += 1;
    return _handler();
  }

  @override
  Future<Result<List<ReservationListItem>>> getMyReservations() async {
    return const Success(<ReservationListItem>[]);
  }
}
