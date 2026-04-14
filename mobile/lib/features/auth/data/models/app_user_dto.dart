import 'package:beachgo/features/auth/domain/entities/app_user.dart';

class AppUserDto {
  const AppUserDto({
    required this.id,
    required this.email,
    required this.role,
    required this.accountType,
    required this.firstName,
    required this.lastName,
    required this.name,
    required this.phone,
  });

  final int? id;
  final String email;
  final String role;
  final String accountType;
  final String firstName;
  final String lastName;
  final String name;
  final String phone;

  factory AppUserDto.fromJson(Map<String, dynamic> json) {
    return AppUserDto(
      id: _asInt(json['id']),
      email: _asString(json['email']),
      role: _asString(json['role']),
      accountType: _asString(json['accountType']),
      firstName: _asString(json['firstName']),
      lastName: _asString(json['lastName']),
      name: _asString(json['name']),
      phone: _asString(json['phone']),
    );
  }

  factory AppUserDto.fromDomain(AppUser user) {
    return AppUserDto(
      id: user.id,
      email: user.email,
      role: user.role,
      accountType: user.accountType,
      firstName: user.firstName,
      lastName: user.lastName,
      name: user.name,
      phone: user.phone,
    );
  }

  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'id': id,
      'email': email,
      'role': role,
      'accountType': accountType,
      'firstName': firstName,
      'lastName': lastName,
      'name': name,
      'phone': phone,
    };
  }

  static int? _asInt(Object? value) {
    if (value is int) return value;
    if (value is num) return value.toInt();
    return int.tryParse(value?.toString() ?? '');
  }

  static String _asString(Object? value) => value?.toString() ?? '';
}

extension AppUserDtoMapper on AppUserDto {
  AppUser toDomain() {
    return AppUser(
      id: id,
      email: email,
      role: role.isNotEmpty ? role : accountType,
      accountType: accountType,
      firstName: firstName,
      lastName: lastName,
      name: name,
      phone: phone,
    );
  }
}
