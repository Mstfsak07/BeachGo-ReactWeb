import 'package:beachgo/features/stories/domain/entities/story.dart';

class StoryDto {
  const StoryDto({
    required this.id,
    required this.beachId,
    required this.beachName,
    required this.beachImageUrl,
    required this.mediaUrl,
    required this.mediaType,
    required this.caption,
    required this.createdAt,
    required this.expiresAt,
  });

  final int id;
  final int beachId;
  final String beachName;
  final String beachImageUrl;
  final String mediaUrl;
  final String mediaType;
  final String caption;
  final DateTime createdAt;
  final DateTime expiresAt;

  factory StoryDto.fromJson(Map<String, dynamic> json) {
    final mediaUrl = _asString(json['mediaUrl']);
    return StoryDto(
      id: _asInt(json['id']) ?? 0,
      beachId: _asInt(json['beachId']) ?? 0,
      beachName: _asString(json['beachName']),
      beachImageUrl: _asString(json['beachImageUrl']),
      mediaUrl: mediaUrl,
      mediaType: _normalizeMediaType(json['mediaType'], mediaUrl),
      caption: _asString(json['caption']),
      createdAt: _asDateTime(json['createdAt']) ?? DateTime.now().toUtc(),
      expiresAt: _asDateTime(json['expiresAt']) ?? DateTime.now().toUtc(),
    );
  }

  static int? _asInt(Object? value) {
    if (value is int) return value;
    if (value is num) return value.toInt();
    return int.tryParse(value?.toString() ?? '');
  }

  static String _asString(Object? value) => value?.toString() ?? '';

  static DateTime? _asDateTime(Object? value) {
    if (value is String && value.isNotEmpty) {
      return DateTime.tryParse(value)?.toUtc();
    }
    return null;
  }

  static String _normalizeMediaType(Object? rawType, String mediaUrl) {
    final normalized = rawType?.toString().trim().toLowerCase();
    if (normalized == 'video') return 'video';
    if (normalized == 'image' || normalized == 'photo') return 'image';

    final loweredUrl = mediaUrl.toLowerCase();
    if (loweredUrl.endsWith('.mp4') ||
        loweredUrl.endsWith('.mov') ||
        loweredUrl.endsWith('.webm') ||
        loweredUrl.endsWith('.m3u8')) {
      return 'video';
    }

    return 'image';
  }
}

extension StoryDtoMapper on StoryDto {
  Story toDomain() {
    return Story(
      id: id,
      beachId: beachId,
      beachName: beachName,
      beachImageUrl: beachImageUrl,
      mediaUrl: mediaUrl,
      mediaType: mediaType,
      caption: caption,
      createdAt: createdAt,
      expiresAt: expiresAt,
    );
  }
}
