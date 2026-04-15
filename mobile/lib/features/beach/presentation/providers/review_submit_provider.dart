import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/features/beach/data/repository/beach_repository.dart';

class ReviewSubmitState {
  const ReviewSubmitState({
    this.isSubmitting = false,
    this.errorMessage,
  });

  final bool isSubmitting;
  final String? errorMessage;

  ReviewSubmitState copyWith({
    bool? isSubmitting,
    String? errorMessage,
    bool clearError = false,
  }) {
    return ReviewSubmitState(
      isSubmitting: isSubmitting ?? this.isSubmitting,
      errorMessage: clearError ? null : (errorMessage ?? this.errorMessage),
    );
  }
}

final reviewSubmitProvider = StateNotifierProvider.autoDispose
    .family<ReviewSubmitNotifier, ReviewSubmitState, int>((ref, beachId) {
  return ReviewSubmitNotifier(ref, beachId);
});

class ReviewSubmitNotifier extends StateNotifier<ReviewSubmitState> {
  ReviewSubmitNotifier(this._ref, this._beachId)
      : super(const ReviewSubmitState());

  final Ref _ref;
  final int _beachId;

  void clearError() {
    if (state.errorMessage != null) {
      state = state.copyWith(clearError: true);
    }
  }

  Future<Failure?> submit({
    required String userName,
    required String userPhone,
    required int rating,
    required String comment,
  }) async {
    if (state.isSubmitting) {
      return null;
    }

    state = const ReviewSubmitState(isSubmitting: true);
    final result = await _ref.read(beachRepositoryProvider).createReview(
          beachId: _beachId,
          userName: userName,
          userPhone: userPhone,
          rating: rating,
          comment: comment,
        );

    return switch (result) {
      Success<void>() => () {
          state = const ReviewSubmitState();
          return null;
        }(),
      FailureResult<void>(failure: final failure) => () {
          state = ReviewSubmitState(
            isSubmitting: false,
            errorMessage: failure.message,
          );
          return failure;
        }(),
    };
  }
}
