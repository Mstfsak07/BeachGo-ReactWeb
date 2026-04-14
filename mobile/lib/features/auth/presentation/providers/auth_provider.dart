import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/features/auth/data/repositories/auth_repository.dart';
import 'package:beachgo/features/auth/domain/entities/auth_session.dart';
import 'package:beachgo/features/auth/domain/entities/app_user.dart';

part 'auth_provider.g.dart';

class AuthState {
  const AuthState({this.user, this.loading = false});

  final AppUser? user;
  final bool loading;

  bool get isAuthenticated => user != null;
  bool get isBusiness => user?.isBusiness ?? false;

  AuthState copyWith({
    AppUser? user,
    bool? loading,
    bool clearUser = false,
  }) {
    return AuthState(
      user: clearUser ? null : (user ?? this.user),
      loading: loading ?? this.loading,
    );
  }
}

@riverpod
class Auth extends _$Auth {
  @override
  Future<AuthState> build() async {
    return _initializeAuth();
  }

  Future<AuthState> _initializeAuth() async {
    final authRepository = ref.read(authRepositoryProvider);
    final refreshResult = await authRepository.refreshSession();

    if (refreshResult case Success<AuthSession>(data: final session)) {
      await authRepository.persistSession(session);
      return AuthState(user: session.user);
    }

    final storedUserResult = await authRepository.getStoredUser();
    if (storedUserResult case Success<AppUser?>(data: final storedUser)) {
      if (storedUser != null) {
        return AuthState(user: storedUser);
      }
    }

    await authRepository.clearSession();
    return const AuthState();
  }

  Future<void> login(String email, String password) async {
    final authRepository = ref.read(authRepositoryProvider);
    state = const AsyncLoading();

    state = await AsyncValue.guard(() async {
      final result = await authRepository.login(email: email, password: password);
      return switch (result) {
        Success<AuthSession>(data: final session) => () async {
            await authRepository.persistSession(session);
            return AuthState(
              user: session.user ??
                  AppUser(
                    id: null,
                    email: email,
                    role: '',
                    accountType: '',
                    firstName: '',
                    lastName: '',
                    name: '',
                    phone: '',
                  ),
            );
          }(),
        FailureResult<AuthSession>(failure: final failure) => throw failure,
      };
    });
  }

  Future<void> register({
    required String name,
    required String email,
    required String password,
  }) async {
    final authRepository = ref.read(authRepositoryProvider);
    state = const AsyncLoading();

    state = await AsyncValue.guard(() async {
      final result = await authRepository.register(
        name: name,
        email: email,
        password: password,
      );

      return switch (result) {
        Success<void>() => const AuthState(),
        FailureResult<void>(failure: final failure) => throw failure,
      };
    });
  }

  Future<void> logout() async {
    await ref.read(authRepositoryProvider).clearSession();
    state = const AsyncData(AuthState());
  }

  Future<void> updateUser(AppUser user) async {
    await ref.read(authRepositoryProvider).updateStoredUser(user);
    final current = state.valueOrNull ?? const AuthState();
    state = AsyncData(current.copyWith(user: user));
  }
}

@riverpod
AppUser? currentUser(Ref ref) {
  return ref.watch(authProvider).valueOrNull?.user;
}

@riverpod
bool isAuthenticated(Ref ref) {
  return ref.watch(authProvider).valueOrNull?.isAuthenticated ?? false;
}
