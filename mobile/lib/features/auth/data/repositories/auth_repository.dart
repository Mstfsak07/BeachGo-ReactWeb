import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/core/network/api_response.dart';
import 'package:beachgo/core/network/dio_client.dart';
import 'package:beachgo/core/storage/storage_service.dart';
import 'package:beachgo/features/auth/data/models/app_user_dto.dart';
import 'package:beachgo/features/auth/data/models/auth_session_dto.dart';
import 'package:beachgo/features/auth/data/models/login_request_dto.dart';
import 'package:beachgo/features/auth/data/models/register_request_dto.dart';
import 'package:beachgo/features/auth/domain/entities/auth_session.dart';
import 'package:beachgo/features/auth/domain/entities/app_user.dart';

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  return AuthRepositoryImpl(ref.watch(dioProvider));
});

abstract class AuthRepository {
  Future<Result<AuthSession>> login({
    required String email,
    required String password,
  });

  Future<Result<void>> register({
    required String name,
    required String email,
    required String password,
  });

  Future<Result<AuthSession>> refreshSession();

  Future<Result<AppUser?>> getStoredUser();

  Future<void> persistSession(AuthSession session);

  Future<void> clearSession();

  Future<void> updateStoredUser(AppUser user);
}

class AuthRepositoryImpl implements AuthRepository {
  AuthRepositoryImpl(this._dio);

  final Dio _dio;

  @override
  Future<Result<AuthSession>> login({
    required String email,
    required String password,
  }) {
    return _execute<AuthSession>(
      request: () => _dio.post(
        '/Auth/login',
        data: LoginRequestDto(email: email, password: password).toJson(),
      ),
      parser: (raw) => AuthSessionDto.fromJson(_asMap(raw)).toDomain(),
    );
  }

  @override
  Future<Result<void>> register({
    required String name,
    required String email,
    required String password,
  }) async {
    try {
      final response = await _dio.post(
        '/Auth/register',
        data: RegisterRequestDto(
          name: name,
          email: email,
          password: password,
        ).toJson(),
      );

      final responseData = response.data;
      if (responseData is! Map<String, dynamic>) {
        return const FailureResult<void>(
          ServerFailure('Invalid response format.'),
        );
      }

      final apiResponse = ApiResponse<Object?>.fromJson(responseData, (raw) => raw);
      if (!apiResponse.success) {
        final message = apiResponse.message.isNotEmpty
            ? apiResponse.message
            : apiResponse.errors.join(', ');
        return FailureResult<void>(
          _mapStatusToFailure(
            response.statusCode,
            message.isNotEmpty ? message : 'Request failed on server.',
          ),
        );
      }

      return const Success<void>(null);
    } on DioException catch (error) {
      return FailureResult<void>(_mapDioException(error));
    } catch (_) {
      return const FailureResult<void>(UnknownFailure());
    }
  }

  @override
  Future<Result<AuthSession>> refreshSession() async {
    final accessToken = await StorageService.getAccessToken();
    final refreshToken = await StorageService.getRefreshToken();

    if (refreshToken == null || refreshToken.isEmpty) {
      return const FailureResult<AuthSession>(
        UnauthorizedFailure('Refresh token not found.'),
      );
    }

    return _execute<AuthSession>(
      request: () => _dio.post(
        '/Auth/refresh',
        data: {
          'accessToken': accessToken ?? '',
          'refreshToken': refreshToken,
        },
        options: Options(headers: {'x-skip-auth-refresh': true}),
      ),
      parser: (raw) => AuthSessionDto.fromJson(_asMap(raw)).toDomain(),
    );
  }

  @override
  Future<Result<AppUser?>> getStoredUser() async {
    try {
      final rawUser = await StorageService.getUser();
      if (rawUser == null || rawUser.isEmpty) {
        return const Success<AppUser?>(null);
      }

      final json = jsonDecode(rawUser);
      if (json is! Map<String, dynamic>) {
        return const FailureResult<AppUser?>(
          UnknownFailure('Stored user payload is invalid.'),
        );
      }

      return Success<AppUser?>(AppUserDto.fromJson(json).toDomain());
    } catch (_) {
      return const FailureResult<AppUser?>(
        UnknownFailure('Stored user payload is invalid.'),
      );
    }
  }

  @override
  Future<void> persistSession(AuthSession session) async {
    await StorageService.setAccessToken(session.accessToken);
    await StorageService.setRefreshToken(session.refreshToken);
    if (session.user != null) {
      final userJson = jsonEncode(AuthSessionDto.fromDomain(session).user?.toJson());
      await StorageService.setUser(userJson);
    }
  }

  @override
  Future<void> clearSession() => StorageService.clearAuthSession();

  @override
  Future<void> updateStoredUser(AppUser user) async {
    final json = jsonEncode(AppUserDto.fromDomain(user).toJson());
    await StorageService.setUser(json);
  }

  Future<Result<T>> _execute<T>({
    required Future<Response<dynamic>> Function() request,
    required T Function(Object? raw) parser,
  }) async {
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
          _mapStatusToFailure(
            response.statusCode,
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
      return FailureResult<T>(_mapDioException(error));
    } catch (_) {
      return FailureResult<T>(const UnknownFailure());
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
      return NetworkFailure(message);
    }
    return UnknownFailure(message);
  }

  String _extractErrorMessage(DioException error) {
    final data = error.response?.data;
    if (data is Map<String, dynamic>) {
      final apiResponse = ApiResponse<Object?>.fromJson(data, (raw) => raw);
      if (apiResponse.message.isNotEmpty) return apiResponse.message;
      if (apiResponse.errors.isNotEmpty) return apiResponse.errors.join(', ');
    }
    return error.message ?? 'Request failed.';
  }

  Map<String, dynamic> _asMap(Object? raw) {
    if (raw is Map<String, dynamic>) return raw;
    return const <String, dynamic>{};
  }
}
