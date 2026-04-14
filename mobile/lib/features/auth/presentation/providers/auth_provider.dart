import 'dart:convert';
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:beachgo/core/models/models.dart';
import 'package:beachgo/core/network/api_response.dart';
import 'package:beachgo/core/network/dio_client.dart';
import 'package:beachgo/core/storage/storage_service.dart';

part 'auth_provider.g.dart';

// ── Auth State ────────────────────────────────────────────────────────────────

class AuthState {
  final AppUser? user;
  final bool loading;

  const AuthState({this.user, this.loading = false});

  bool get isAuthenticated => user != null;
  bool get isBusiness => user?.isBusiness ?? false;

  AuthState copyWith({AppUser? user, bool? loading, bool clearUser = false}) {
    return AuthState(
      user: clearUser ? null : (user ?? this.user),
      loading: loading ?? this.loading,
    );
  }
}

// ── Auth Notifier ─────────────────────────────────────────────────────────────
// Web'deki AuthContext + AuthProvider'ın Riverpod karşılığı.

@riverpod
class Auth extends _$Auth {
  @override
  Future<AuthState> build() async {
    // Uygulama açılırken token refresh dene (web'deki initializeAuth)
    return _initializeAuth();
  }

  Future<AuthState> _initializeAuth() async {
    final dio = ref.read(dioProvider);
    try {
      final accessToken = await StorageService.getAccessToken();
      final refreshToken = await StorageService.getRefreshToken();

      if (refreshToken == null) return const AuthState();

      final response = await dio.post('/Auth/refresh', data: {
        'accessToken': accessToken ?? '',
        'refreshToken': refreshToken,
      });

      final auth = _parseAuthResponse(response);
      if (auth?.accessToken != null) {
        await _persistAuth(auth!);
        return AuthState(user: auth.user);
      }
    } catch (_) {
      await StorageService.clearAuthSession();
    }
    return const AuthState();
  }

  Future<void> login(String email, String password) async {
    final dio = ref.read(dioProvider);
    state = const AsyncLoading();

    state = await AsyncValue.guard(() async {
      final response = await dio.post('/Auth/login', data: {
        'email': email,
        'password': password,
      });

      final auth = _parseAuthResponse(response);
      if (auth?.accessToken == null) {
        throw const ApiException('Giriş bilgileri hatalı.');
      }

      await _persistAuth(auth!);
      return AuthState(
        user: auth.user ?? AppUser(email: email, role: auth.role),
      );
    });
  }

  Future<void> register({
    required String name,
    required String email,
    required String password,
  }) async {
    final dio = ref.read(dioProvider);
    state = const AsyncLoading();

    state = await AsyncValue.guard(() async {
      await dio.post('/Auth/register', data: {
        'name': name,
        'email': email,
        'password': password,
      });
      return const AuthState(); // Kayıt sonrası login gerekli
    });
  }

  Future<void> logout() async {
    await StorageService.clearAuthSession();
    state = const AsyncData(AuthState());
  }

  Future<void> updateUser(AppUser user) async {
    await StorageService.setUser(jsonEncode(user.toJson()));
    final current = state.valueOrNull ?? const AuthState();
    state = AsyncData(current.copyWith(user: user));
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  AuthResponse? _parseAuthResponse(Response response) {
    try {
      final raw = response.data as Map<String, dynamic>?;
      if (raw == null) return null;

      final data = (raw['data'] as Map<String, dynamic>?) ?? raw;

      return AuthResponse(
        accessToken: data['accessToken'] as String? ??
            data['token'] as String? ??
            data['Token'] as String?,
        refreshToken: data['refreshToken'] as String? ??
            data['RefreshToken'] as String?,
        role: data['role'] as String? ?? data['accountType'] as String?,
        user: data['user'] != null
            ? AppUser.fromJson(data['user'] as Map<String, dynamic>)
            : data['User'] != null
                ? AppUser.fromJson(data['User'] as Map<String, dynamic>)
                : null,
      );
    } catch (_) {
      return null;
    }
  }

  Future<void> _persistAuth(AuthResponse auth) async {
    if (auth.accessToken != null) {
      await StorageService.setAccessToken(auth.accessToken!);
    }
    if (auth.refreshToken != null) {
      await StorageService.setRefreshToken(auth.refreshToken!);
    }
    if (auth.user != null) {
      await StorageService.setUser(jsonEncode(auth.user!.toJson()));
    }
  }
}

// Kolaylık provider'ları — ekranlarda doğrudan kullanılır
@riverpod
AppUser? currentUser(Ref ref) {
  return ref.watch(authProvider).valueOrNull?.user;
}

@riverpod
bool isAuthenticated(Ref ref) {
  return ref.watch(authProvider).valueOrNull?.isAuthenticated ?? false;
}
