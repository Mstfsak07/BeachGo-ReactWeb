import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/features/reservation/data/repositories/reservation_repository.dart';
import 'package:beachgo/features/reservation/domain/entities/reservation_confirmation.dart';

enum ReservationSubmitStatus {
  idle,
  submitting,
  success,
  failure,
}

class ReservationSubmitState {
  const ReservationSubmitState({
    this.status = ReservationSubmitStatus.idle,
    this.errorMessage,
    this.confirmation,
  });

  final ReservationSubmitStatus status;
  final String? errorMessage;
  final ReservationConfirmation? confirmation;

  bool get isSubmitting => status == ReservationSubmitStatus.submitting;

  ReservationSubmitState copyWith({
    ReservationSubmitStatus? status,
    String? errorMessage,
    bool clearError = false,
    ReservationConfirmation? confirmation,
    bool clearConfirmation = false,
  }) {
    return ReservationSubmitState(
      status: status ?? this.status,
      errorMessage: clearError ? null : (errorMessage ?? this.errorMessage),
      confirmation: clearConfirmation
          ? null
          : (confirmation ?? this.confirmation),
    );
  }
}

final reservationSubmitProvider = StateNotifierProvider.autoDispose
    .family<ReservationSubmitController, ReservationSubmitState, int>(
  (ref, beachId) {
    return ReservationSubmitController(
      ref.watch(reservationRepositoryProvider),
    );
  },
);

class ReservationSubmitController extends StateNotifier<ReservationSubmitState> {
  ReservationSubmitController(this._repository)
      : super(const ReservationSubmitState());

  final ReservationRepository _repository;

  Future<ReservationConfirmation?> submit({
    required int beachId,
    required DateTime reservationDate,
    required String reservationTime,
    required int guestCount,
    required String note,
  }) async {
    if (state.isSubmitting) {
      return null;
    }

    state = state.copyWith(
      status: ReservationSubmitStatus.submitting,
      clearError: true,
      clearConfirmation: true,
    );

    final result = await _repository.createReservation(
      beachId: beachId,
      reservationDate: reservationDate,
      reservationTime: reservationTime,
      guestCount: guestCount,
      note: note,
    );

    return switch (result) {
      Success<ReservationConfirmation>(data: final confirmation) => () {
          state = ReservationSubmitState(
            status: ReservationSubmitStatus.success,
            confirmation: confirmation,
          );
          return confirmation;
        }(),
      FailureResult<ReservationConfirmation>(failure: final failure) => () {
          state = ReservationSubmitState(
            status: ReservationSubmitStatus.failure,
            errorMessage: _friendlyMessage(failure),
          );
          return null;
        }(),
    };
  }

  void clearError() {
    if (state.errorMessage == null &&
        state.status != ReservationSubmitStatus.failure) {
      return;
    }

    state = state.copyWith(
      status: ReservationSubmitStatus.idle,
      clearError: true,
    );
  }

  String _friendlyMessage(Failure failure) {
    if (failure.message.trim().isNotEmpty) {
      return failure.message;
    }

    return switch (failure) {
      NetworkFailure() => 'Internet baglantinizi kontrol edip tekrar deneyin.',
      UnauthorizedFailure() =>
        'Rezervasyon icin giris yapmaniz veya hesabinizi dogrulamaniz gerekiyor.',
      ServerFailure() => 'Rezervasyon su anda olusturulamadi. Lutfen tekrar deneyin.',
      NotFoundFailure() => 'Secilen plaj bilgisi bulunamadi.',
      UnknownFailure() => 'Beklenmeyen bir hata olustu. Lutfen tekrar deneyin.',
    };
  }
}
