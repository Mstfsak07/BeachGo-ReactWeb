import 'package:flutter/foundation.dart';

// Ortam bazlı konfigürasyon
// Kullanım:
// flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5143/api
// flutter run --dart-define=ENV=production

class AppConfig {
  static const String _env = String.fromEnvironment('ENV', defaultValue: 'development');
  static const String _baseUrlOverride = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: '',
  );
  static const String _apiHostOverride = String.fromEnvironment(
    'API_HOST',
    defaultValue: '',
  );
  static const String _apiPortOverride = String.fromEnvironment(
    'API_PORT',
    defaultValue: '5143',
  );

  static bool get isProduction => _env == 'production';

  static String get baseUrl {
    if (_baseUrlOverride.trim().isNotEmpty) {
      return _normalize(_baseUrlOverride);
    }

    if (isProduction) {
      return 'https://api.beachgo.net/api';
    }

    final host = _resolvedHost;
    final port = _apiPortOverride.trim().isEmpty ? '5143' : _apiPortOverride.trim();
    return 'http://$host:$port/api';
  }

  static const Duration connectTimeout = Duration(seconds: 10);
  static const Duration receiveTimeout = Duration(seconds: 10);

  static String get _resolvedHost {
    if (_apiHostOverride.trim().isNotEmpty) {
      return _apiHostOverride.trim();
    }

    switch (defaultTargetPlatform) {
      case TargetPlatform.android:
        return '10.0.2.2';
      case TargetPlatform.iOS:
      case TargetPlatform.macOS:
      case TargetPlatform.windows:
      case TargetPlatform.linux:
      case TargetPlatform.fuchsia:
        return 'localhost';
    }
  }

  static String _normalize(String url) {
    final trimmed = url.trim();
    return trimmed.endsWith('/') ? trimmed.substring(0, trimmed.length - 1) : trimmed;
  }
}
