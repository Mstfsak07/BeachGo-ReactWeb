import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../network/app_config.dart';
import '../storage/storage_service.dart';

// Web'deki api/axios.ts'nin Flutter karşılığı.
// Token ekleme + 401 üzerinde otomatik refresh + queue mekanizması.

final dioProvider = Provider<Dio>((ref) {
  final dio = Dio(
    BaseOptions(
      baseUrl: AppConfig.baseUrl,
      connectTimeout: AppConfig.connectTimeout,
      receiveTimeout: AppConfig.receiveTimeout,
      headers: {'Content-Type': 'application/json'},
    ),
  );

  dio.interceptors.add(AuthInterceptor(dio));
  return dio;
});

class AuthInterceptor extends QueuedInterceptor {
  final Dio _dio;
  bool _isRefreshing = false;

  AuthInterceptor(this._dio);

  @override
  Future<void> onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final token = await StorageService.getAccessToken();
    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    handler.next(options);
  }

  @override
  Future<void> onError(
    DioException err,
    ErrorInterceptorHandler handler,
  ) async {
    if (err.response?.statusCode == 401 && !_isRefreshing) {
      _isRefreshing = true;
      try {
        final newToken = await _refreshToken();
        if (newToken != null) {
          // Başarısız isteği yeni token ile tekrar gönder
          final opts = err.requestOptions;
          opts.headers['Authorization'] = 'Bearer $newToken';
          final response = await _dio.fetch(opts);
          handler.resolve(response);
          return;
        }
      } catch (_) {
        await StorageService.clearAuthSession();
        // Router'a logout eventi göndermek için AppRouter'daki listener tetiklenir
      } finally {
        _isRefreshing = false;
      }
    }
    handler.next(err);
  }

  Future<String?> _refreshToken() async {
    final accessToken = await StorageService.getAccessToken();
    final refreshToken = await StorageService.getRefreshToken();

    if (refreshToken == null) return null;

    // Refresh için interceptor bypass eden ayrı Dio instance
    final refreshDio = Dio(BaseOptions(baseUrl: AppConfig.baseUrl));
    final response = await refreshDio.post('/Auth/refresh', data: {
      'accessToken': accessToken ?? '',
      'refreshToken': refreshToken,
    });

    final data = _unwrapData(response.data);
    final newToken = data['accessToken'] ?? data['token'] ?? data['Token'];
    final newRefresh = data['refreshToken'] ?? data['RefreshToken'];

    if (newToken is String) {
      await StorageService.setAccessToken(newToken);
      if (newRefresh is String) {
        await StorageService.setRefreshToken(newRefresh);
      }
      return newToken;
    }
    return null;
  }

  Map<String, dynamic> _unwrapData(dynamic raw) {
    if (raw is Map<String, dynamic>) {
      return (raw['data'] as Map<String, dynamic>?) ?? raw;
    }
    return {};
  }
}
