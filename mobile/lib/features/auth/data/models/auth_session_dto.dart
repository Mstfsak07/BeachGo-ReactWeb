import 'package:beachgo/features/auth/data/models/app_user_dto.dart';
import 'package:beachgo/features/auth/domain/entities/auth_session.dart';

class AuthSessionDto {
  const AuthSessionDto({
    required this.accessToken,
    required this.refreshToken,
    required this.user,
  });

  final String accessToken;
  final String refreshToken;
  final AppUserDto? user;

  factory AuthSessionDto.fromJson(Map<String, dynamic> json) {
    final role = _asString(json['role']);
    final accountType = _asString(json['accountType']);
    final rawUser = json['user'] ?? json['User'];
    AppUserDto? user;

    if (rawUser is Map<String, dynamic>) {
      user = AppUserDto.fromJson(rawUser);
    } else if (role.isNotEmpty || accountType.isNotEmpty || _asString(json['email']).isNotEmpty) {
      user = AppUserDto(
        id: _asInt(json['id']),
        email: _asString(json['email']),
        role: role,
        accountType: accountType,
        firstName: _asString(json['firstName']),
        lastName: _asString(json['lastName']),
        name: _asString(json['name']),
        phone: _asString(json['phone']),
      );
    }

    return AuthSessionDto(
      accessToken: _asString(json['accessToken']).isNotEmpty
          ? _asString(json['accessToken'])
          : _asString(json['token']).isNotEmpty
              ? _asString(json['token'])
              : _asString(json['Token']),
      refreshToken: _asString(json['refreshToken']).isNotEmpty
          ? _asString(json['refreshToken'])
          : _asString(json['RefreshToken']),
      user: user,
    );
  }

  factory AuthSessionDto.fromDomain(AuthSession session) {
    return AuthSessionDto(
      accessToken: session.accessToken,
      refreshToken: session.refreshToken,
      user: session.user != null ? AppUserDto.fromDomain(session.user!) : null,
    );
  }

  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'accessToken': accessToken,
      'refreshToken': refreshToken,
      if (user != null) 'user': user!.toJson(),
    };
  }

  static int? _asInt(Object? value) {
    if (value is int) return value;
    if (value is num) return value.toInt();
    return int.tryParse(value?.toString() ?? '');
  }

  static String _asString(Object? value) => value?.toString() ?? '';
}

extension AuthSessionDtoMapper on AuthSessionDto {
  AuthSession toDomain() {
    return AuthSession(
      accessToken: accessToken,
      refreshToken: refreshToken,
      user: user?.toDomain(),
    );
  }
}
