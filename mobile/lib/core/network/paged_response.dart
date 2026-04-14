class PagedResponse<T> {
  const PagedResponse({
    required this.items,
    required this.page,
    required this.pageSize,
    required this.totalCount,
  });

  final List<T> items;
  final int page;
  final int pageSize;
  final int totalCount;

  factory PagedResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic> itemJson) itemParser,
  ) {
    final rawItems = json['items'];
    final items = rawItems is List
        ? rawItems
            .whereType<Map<String, dynamic>>()
            .map(itemParser)
            .toList(growable: false)
        : <T>[];

    return PagedResponse<T>(
      items: items,
      page: _asInt(json['page']) ?? _asInt(json['currentPage']) ?? 1,
      pageSize: _asInt(json['pageSize']) ?? items.length,
      totalCount: _asInt(json['totalCount']) ?? items.length,
    );
  }

  static int? _asInt(Object? value) {
    if (value is int) return value;
    if (value is num) return value.toInt();
    return int.tryParse(value?.toString() ?? '');
  }
}
