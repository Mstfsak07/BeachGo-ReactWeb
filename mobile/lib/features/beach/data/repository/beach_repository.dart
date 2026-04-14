import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/core/network/api_response.dart';
import 'package:beachgo/core/network/dio_client.dart';
import 'package:beachgo/core/network/paged_response.dart';
import 'package:beachgo/features/beach/data/models/beach_dto.dart';
import 'package:beachgo/features/beach/data/models/beach_filter_dto.dart';
import 'package:beachgo/features/beach/data/models/weather_dto.dart';
import 'package:beachgo/features/beach/domain/entities/beach.dart';
import 'package:beachgo/features/beach/domain/entities/beach_filter.dart';
import 'package:beachgo/features/beach/domain/entities/weather.dart';

final beachRepositoryProvider = Provider<BeachRepository>((ref) {
  return BeachRepositoryImpl(ref.watch(dioProvider));
});

abstract class BeachRepository {
  Future<Result<PagedResponse<Beach>>> getBeaches({
    int page = 1,
    int pageSize = 20,
  });

  Future<Result<Beach>> getBeachById(int id);

  Future<Result<List<Beach>>> searchBeaches(String query);

  Future<Result<List<Beach>>> filterBeaches(BeachFilter filter);

  Future<Result<Weather>> getBeachWeather(int id);
}

class BeachRepositoryImpl implements BeachRepository {
  BeachRepositoryImpl(this._dio);

  final Dio _dio;

  @override
  Future<Result<PagedResponse<Beach>>> getBeaches({
    int page = 1,
    int pageSize = 20,
  }) {
    return _execute<PagedResponse<Beach>>(
      request: () => _dio.get(
        '/Beaches',
        queryParameters: {'page': page, 'pageSize': pageSize},
      ),
      parser: (raw) {
        final json = _asMap(raw);
        return PagedResponse<Beach>.fromJson(
          json,
          (itemJson) => BeachDto.fromJson(itemJson).toDomain(),
        );
      },
    );
  }

  @override
  Future<Result<Beach>> getBeachById(int id) {
    return _execute<Beach>(
      request: () => _dio.get('/Beaches/$id'),
      parser: (raw) => BeachDto.fromJson(_asMap(raw)).toDomain(),
    );
  }

  @override
  Future<Result<List<Beach>>> searchBeaches(String query) {
    return _execute<List<Beach>>(
      request: () => _dio.get(
        '/Beaches/search',
        queryParameters: {'q': query.trim()},
      ),
      parser: (raw) => _parseBeachList(raw),
    );
  }

  @override
  Future<Result<List<Beach>>> filterBeaches(BeachFilter filter) {
    return _execute<List<Beach>>(
      request: () => _dio.post('/Beaches/filter', data: filter.toDto().toJson()),
      parser: (raw) => _parseBeachList(raw),
    );
  }

  @override
  Future<Result<Weather>> getBeachWeather(int id) {
    return _execute<Weather>(
      request: () => _dio.get('/Beaches/$id/weather'),
      parser: (raw) => WeatherDto.fromJson(_asMap(raw)).toDomain(),
    );
  }

  Future<Result<T>> _execute<T>({
    required Future<Response<dynamic>> Function() request,
    required T Function(Object? raw) parser,
    int maxRetries = 1,
  }) async {
    var attempt = 0;

    while (true) {
      try {
        final response = await request();
        final responseData = response.data;

        if (responseData is! Map<String, dynamic>) {
          return FailureResult<T>(
            const ServerFailure('Invalid response format.'),
          );
        }

        final apiResponse = ApiResponse<T>.fromJson(responseData, parser);
        if (!apiResponse.success) {
          final message = apiResponse.message.isNotEmpty
              ? apiResponse.message
              : apiResponse.errors.join(', ');
          return FailureResult<T>(
            ServerFailure(
              message.isNotEmpty ? message : 'Request failed on server.',
            ),
          );
        }

        final data = apiResponse.data;
        if (data == null) {
          return FailureResult<T>(
            const NotFoundFailure('Response data is empty.'),
          );
        }

        return Success<T>(data);
      } on DioException catch (error) {
        if (_shouldRetry(error) && attempt < maxRetries) {
          attempt += 1;
          await Future<void>.delayed(Duration(milliseconds: 300 * attempt));
          continue;
        }
        return FailureResult<T>(_mapDioException(error));
      } catch (_) {
        return FailureResult<T>(const UnknownFailure());
      }
    }
  }

  List<Beach> _parseBeachList(Object? raw) {
    if (raw is List) {
      return raw
          .whereType<Map<String, dynamic>>()
          .map((item) => BeachDto.fromJson(item).toDomain())
          .toList(growable: false);
    }

    if (raw is Map<String, dynamic> && raw['items'] is List) {
      return (raw['items'] as List)
          .whereType<Map<String, dynamic>>()
          .map((item) => BeachDto.fromJson(item).toDomain())
          .toList(growable: false);
    }

    return const <Beach>[];
  }

  Map<String, dynamic> _asMap(Object? raw) {
    if (raw is Map<String, dynamic>) {
      return raw;
    }
    return const <String, dynamic>{};
  }

  bool _shouldRetry(DioException error) {
    return error.type == DioExceptionType.connectionTimeout ||
        error.type == DioExceptionType.receiveTimeout ||
        error.type == DioExceptionType.sendTimeout ||
        error.type == DioExceptionType.connectionError;
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
    if (_shouldRetry(error) || error.type == DioExceptionType.badCertificate) {
      return NetworkFailure(message);
    }
    if (error.type == DioExceptionType.cancel) {
      return const UnknownFailure('Request was cancelled.');
    }
    return UnknownFailure(message);
  }

  String _extractErrorMessage(DioException error) {
    final data = error.response?.data;
    if (data is Map<String, dynamic>) {
      final apiResponse = ApiResponse<Object?>.fromJson(
        data,
        (raw) => raw,
      );
      if (apiResponse.message.isNotEmpty) {
        return apiResponse.message;
      }
      if (apiResponse.errors.isNotEmpty) {
        return apiResponse.errors.join(', ');
      }
    }

    return error.message ?? 'Request failed.';
  }
}
