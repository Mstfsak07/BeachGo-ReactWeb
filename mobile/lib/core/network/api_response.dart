class ApiResponse<T> {
  const ApiResponse({
    required this.success,
    required this.message,
    required this.data,
    required this.errors,
  });

  final bool success;
  final String message;
  final T? data;
  final List<String> errors;

  factory ApiResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Object? raw) dataParser,
  ) {
    final rawErrors = json['errors'];
    return ApiResponse<T>(
      success: json['success'] as bool? ?? false,
      message: json['message'] as String? ?? '',
      data: json.containsKey('data') ? dataParser(json['data']) : null,
      errors: rawErrors is List
          ? rawErrors.map((error) => error?.toString() ?? '').toList()
          : const <String>[],
    );
  }
}
