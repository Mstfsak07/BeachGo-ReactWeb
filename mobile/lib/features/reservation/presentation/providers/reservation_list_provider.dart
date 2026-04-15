import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/features/reservation/data/repositories/reservation_repository.dart';
import 'package:beachgo/features/reservation/domain/entities/reservation_list_item.dart';

final reservationListProvider =
    FutureProvider<Result<List<ReservationListItem>>>((ref) {
  return ref.watch(reservationRepositoryProvider).getMyReservations();
});
