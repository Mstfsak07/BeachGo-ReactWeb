import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/core/network/api_response.dart';
import 'package:beachgo/core/network/dio_client.dart';
import 'package:beachgo/features/reservation/data/models/create_reservation_request_dto.dart';
import 'package:beachgo/features/reservation/data/models/reservation_confirmation_dto.dart';
import 'package:beachgo/features/reservation/data/models/reservation_list_item_dto.dart';
import 'package:beachgo/features/reservation/domain/entities/reservation_confirmation.dart';
import 'package:beachgo/features/reservation/domain/entities/reservation_list_item.dart';

final reservationRepositoryProvider = Provider<ReservationRepository>((ref) {
  return ReservationRepositoryImpl(ref.watch(dioProvider));
});

abstract class ReservationRepository {
  Future<Result<ReservationConfirmation>> createReservation({
    required int beachId,
    required DateTime reservationDate,
    required String reservationTime,
    required int guestCount,
    required String note,
  });

  Future<Result<List<ReservationListItem>>> getMyReservations();
}

class ReservationRepositoryImpl implements ReservationRepository {
  ReservationRepositoryImpl(this._dio);

  final Dio _dio;

  @override
  Future<Result<ReservationConfirmation>> createReservation({
    required int beachId,
    required DateTime reservationDate,
    required String reservationTime,
    required int guestCount,
    required String note,
  }) {
    return _execute<ReservationConfirmation>(
      request: () => _dio.post(
        '/Reservations',
        data: CreateReservationRequestDto(
          beachId: beachId,
          reservationDate: reservationDate,
          reservationTime: reservationTime,
          personCount: guestCount,
          sunbedCount: 0,
          notes: note.trim(),
          totalPrice: 0,
        ).toJson(),
      ),
      parser: (raw) =>
          ReservationConfirmationDto.fromJson(_asMap(raw)).toDomain(),
    );
  }

  @override
  Future<Result<List<ReservationListItem>>> getMyReservations() {
    return _execute<List<ReservationListItem>>(
      request: () => _dio.get('/Reservations/my'),
      parser: (raw) {
        if (raw is! List) {
          return const <ReservationListItem>[];
        }

        return raw
            .whereType<Map<String, dynamic>>()
            .map((item) => ReservationListItemDto.fromJson(item).toDomain())
            .toList(growable: false);
      },
    );
  }

  Future<Result<T>> _execute<T>({
    required Future<Response<dynamic>> Function() request,
    required T Function(Object? raw) parser,
  }) async {
    try {
      final response = await request();
      final responseData = response.data;
      if (responseData is! Map<String, dynamic>) {
        return FailureResult<T>(const ServerFailure('Gecersiz sunucu yaniti.'));
      }

      final apiResponse = ApiResponse<T>.fromJson(responseData, parser);
      if (!apiResponse.success) {
        final message = apiResponse.message.isNotEmpty
            ? apiResponse.message
            : apiResponse.errors.join(', ');
        return FailureResult<T>(
          _mapStatusToFailure(
            response.statusCode,
            message.isNotEmpty ? message : 'Rezervasyon olusturulamadi.',
          ),
        );
      }

      final data = apiResponse.data;
      if (data == null) {
        return FailureResult<T>(const NotFoundFailure('Sunucu yaniti bos geldi.'));
      }

      return Success<T>(data);
    } on DioException catch (error) {
      return FailureResult<T>(_mapDioException(error));
    } catch (_) {
      return FailureResult<T>(const UnknownFailure('Beklenmeyen bir hata olustu.'));
    }
  }

  Failure _mapStatusToFailure(int? statusCode, String message) {
    if (statusCode == 401 || statusCode == 403) {
      return UnauthorizedFailure(message);
    }
    if (statusCode == 404) {
      return NotFoundFailure(message);
    }
    if (statusCode != null && statusCode >= 500) {
      return ServerFailure(message);
    }
    return ServerFailure(message);
  }

  Failure _mapDioException(DioException error) {
    final statusCode = error.response?.statusCode;
    final message = _extractErrorMessage(error);

    if (statusCode == 401 || statusCode == 403) {
      return UnauthorizedFailure(message);
    }
    if (statusCode == 404) {
      return NotFoundFailure(message);
    }
    if (statusCode != null && statusCode >= 500) {
      return ServerFailure(message);
    }
    if (error.type == DioExceptionType.connectionTimeout ||
        error.type == DioExceptionType.receiveTimeout ||
        error.type == DioExceptionType.sendTimeout ||
        error.type == DioExceptionType.connectionError) {
      return NetworkFailure(
        message.isNotEmpty ? message : 'Internet baglantinizi kontrol edin.',
      );
    }
    if (error.type == DioExceptionType.cancel) {
      return const UnknownFailure('Istek iptal edildi.');
    }
    return UnknownFailure(message.isNotEmpty ? message : 'Istek tamamlanamadi.');
  }

  String _extractErrorMessage(DioException error) {
    final data = error.response?.data;
    if (data is Map<String, dynamic>) {
      final apiResponse = ApiResponse<Object?>.fromJson(data, (raw) => raw);
      if (apiResponse.message.isNotEmpty) {
        return apiResponse.message;
      }
      if (apiResponse.errors.isNotEmpty) {
        return apiResponse.errors.join(', ');
      }
    }
    return error.message ?? 'Rezervasyon istegi basarisiz oldu.';
  }

  Map<String, dynamic> _asMap(Object? raw) {
    if (raw is Map<String, dynamic>) {
      return raw;
    }
    return const <String, dynamic>{};
  }
}
