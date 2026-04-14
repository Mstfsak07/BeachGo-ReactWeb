import 'dart:convert';
import 'dart:developer';

import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/network/app_config.dart';
import 'package:beachgo/core/storage/storage_service.dart';
import 'package:beachgo/core/network/api_response.dart';
import 'package:beachgo/features/auth/data/models/auth_session_dto.dart';

final dioProvider = Provider<Dio>((ref) => DioClient().instance);

class DioClient {
  DioClient._internal();

  static final DioClient _singleton = DioClient._internal();

  factory DioClient() => _singleton;

  late final Dio _dio = _createDio();

  Dio get instance => _dio;

  Dio _createDio() {
    final dio = Dio(
      BaseOptions(
        baseUrl: AppConfig.baseUrl,
        connectTimeout: AppConfig.connectTimeout,
        receiveTimeout: AppConfig.receiveTimeout,
        sendTimeout: AppConfig.connectTimeout,
        headers: const {'Content-Type': 'application/json'},
      ),
    );

    dio.interceptors.add(_AuthInterceptor());
    dio.interceptors.add(_ErrorInterceptor());

    if (kDebugMode) {
      dio.interceptors.add(
        LogInterceptor(
          requestBody: true,
          responseBody: true,
          requestHeader: false,
          responseHeader: false,
        ),
      );
    }

    return dio;
  }
}

class _AuthInterceptor extends QueuedInterceptor {
  bool _isRefreshing = false;

  @override
  Future<void> onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final token = await StorageService.getAccessToken();
    if (token != null && token.isNotEmpty) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    handler.next(options);
  }

  @override
  Future<void> onError(
    DioException err,
    ErrorInterceptorHandler handler,
  ) async {
    final request = err.requestOptions;
    final shouldSkipRefresh =
        request.headers['x-skip-auth-refresh'] == true ||
            request.path.contains('/Auth/login') ||
            request.path.contains('/Auth/register') ||
            request.path.contains('/Auth/refresh');

    if (err.response?.statusCode == 401 && !shouldSkipRefresh && !_isRefreshing) {
      _isRefreshing = true;
      try {
        final refreshed = await _refreshSession();
        if (refreshed != null) {
          request.headers['Authorization'] = 'Bearer ${refreshed.accessToken}';
          final retryDio = DioClient().instance;
          final response = await retryDio.fetch<dynamic>(request);
          handler.resolve(response);
          return;
        }
      } catch (_) {
        await StorageService.clearAuthSession();
      } finally {
        _isRefreshing = false;
      }
    }

    handler.next(err);
  }

  Future<AuthSessionDto?> _refreshSession() async {
    final accessToken = await StorageService.getAccessToken();
    final refreshToken = await StorageService.getRefreshToken();
    if (refreshToken == null || refreshToken.isEmpty) return null;

    final refreshDio = Dio(
      BaseOptions(
        baseUrl: AppConfig.baseUrl,
        connectTimeout: AppConfig.connectTimeout,
        receiveTimeout: AppConfig.receiveTimeout,
        sendTimeout: AppConfig.connectTimeout,
        headers: const {'Content-Type': 'application/json'},
      ),
    );

    final response = await refreshDio.post<Map<String, dynamic>>(
      '/Auth/refresh',
      data: {
        'accessToken': accessToken ?? '',
        'refreshToken': refreshToken,
      },
      options: Options(headers: {'x-skip-auth-refresh': true}),
    );

    final payload = response.data;
    if (payload == null) return null;

    final apiResponse = ApiResponse<AuthSessionDto>.fromJson(
      payload,
      (raw) => AuthSessionDto.fromJson(raw as Map<String, dynamic>),
    );

    if (!apiResponse.success || apiResponse.data == null) {
      await StorageService.clearAuthSession();
      return null;
    }

    final session = apiResponse.data!;
    await StorageService.setAccessToken(session.accessToken);
    await StorageService.setRefreshToken(session.refreshToken);
    if (session.user != null) {
      await StorageService.setUser(jsonEncode(session.user!.toJson()));
    }
    return session;
  }
}

class _ErrorInterceptor extends Interceptor {
  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    if (kDebugMode) {
      log(
        'Dio error: ${err.requestOptions.method} ${err.requestOptions.uri} -> ${err.response?.statusCode}',
        error: err.message,
      );
    }
    handler.next(err);
  }
}
