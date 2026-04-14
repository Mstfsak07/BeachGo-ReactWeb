import 'package:dio/dio.dart';

// Web'deki unwrapResponse / unwrapArrayResponse'un Dart karşılığı.
// API'nin hem { success, data } zarflı hem de düz array döndürebildiği
// tutarsız davranışını merkezi olarak çözer.

class ApiException implements Exception {
  final String message;
  final int? statusCode;

  const ApiException(this.message, {this.statusCode});

  @override
  String toString() => 'ApiException: $message';
}

T unwrapResponse<T>(
  Response response,
  T Function(dynamic json) fromJson,
) {
  final raw = response.data;

  if (raw is Map<String, dynamic>) {
    if (raw['success'] == false) {
      throw ApiException(raw['message'] ?? 'Bir hata oluştu');
    }
    final data = raw.containsKey('data') ? raw['data'] : raw;
    return fromJson(data);
  }

  return fromJson(raw);
}

List<T> unwrapListResponse<T>(
  Response response,
  T Function(Map<String, dynamic> json) fromJson,
) {
  final raw = response.data;

  dynamic data;
  if (raw is Map<String, dynamic>) {
    if (raw['success'] == false) {
      throw ApiException(raw['message'] ?? 'Bir hata oluştu');
    }
    data = raw['data'] ?? raw;
  } else {
    data = raw;
  }

  if (data is List) {
    return data.map((e) => fromJson(e as Map<String, dynamic>)).toList();
  }

  if (data is Map<String, dynamic> && data['items'] is List) {
    return (data['items'] as List)
        .map((e) => fromJson(e as Map<String, dynamic>))
        .toList();
  }

  return [];
}

// Hata mesajını kullanıcıya gösterilecek formata çevirir
String friendlyError(Object error) {
  if (error is ApiException) return error.message;
  if (error is DioException) {
    final data = error.response?.data;
    if (data is Map && data['message'] != null) {
      return data['message'].toString();
    }
    switch (error.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.receiveTimeout:
        return 'Bağlantı zaman aşımına uğradı.';
      case DioExceptionType.connectionError:
        return 'İnternet bağlantısı yok.';
      default:
        return 'Bir hata oluştu. Lütfen tekrar deneyin.';
    }
  }
  return 'Beklenmeyen bir hata oluştu.';
}
