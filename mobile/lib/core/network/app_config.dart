// Ortam bazlı konfigürasyon
// Kullanım: flutter run --dart-define=ENV=production

class AppConfig {
  static const String _env = String.fromEnvironment('ENV', defaultValue: 'development');

  static bool get isProduction => _env == 'production';

  static String get baseUrl {
    if (isProduction) {
      return 'https://api.beachgo.com/api';
    }
    return 'http://10.0.2.2:5000/api'; // Android emülatör için localhost
    // iOS simülatör için: 'http://localhost:5000/api'
  }

  static const Duration connectTimeout = Duration(seconds: 10);
  static const Duration receiveTimeout = Duration(seconds: 10);
}
