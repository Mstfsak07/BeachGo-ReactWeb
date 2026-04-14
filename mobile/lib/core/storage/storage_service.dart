import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

// Token'lar güvenli depoda, kullanıcı tercihleri SharedPreferences'ta tutulur.
// Web'deki storage.ts abstraction'ının Flutter karşılığı.

class StorageService {
  static const _storage = FlutterSecureStorage(
    aOptions: AndroidOptions(encryptedSharedPreferences: true),
  );

  static const _kAccessToken = 'accessToken';
  static const _kRefreshToken = 'refreshToken';
  static const _kUser = 'user';

  // ── Token işlemleri ──────────────────────────────────────────

  static Future<String?> getAccessToken() => _storage.read(key: _kAccessToken);
  static Future<String?> getRefreshToken() => _storage.read(key: _kRefreshToken);

  static Future<void> setAccessToken(String token) =>
      _storage.write(key: _kAccessToken, value: token);

  static Future<void> setRefreshToken(String token) =>
      _storage.write(key: _kRefreshToken, value: token);

  static Future<String?> getUser() => _storage.read(key: _kUser);

  static Future<void> setUser(String userJson) =>
      _storage.write(key: _kUser, value: userJson);

  static Future<void> clearAuthSession() async {
    await Future.wait([
      _storage.delete(key: _kAccessToken),
      _storage.delete(key: _kRefreshToken),
      _storage.delete(key: _kUser),
    ]);
  }

  // ── Kullanıcı tercihleri ─────────────────────────────────────

  static Future<bool> getThemeMode() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getBool('darkMode') ?? false;
  }

  static Future<void> setThemeMode(bool isDark) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool('darkMode', isDark);
  }
}
