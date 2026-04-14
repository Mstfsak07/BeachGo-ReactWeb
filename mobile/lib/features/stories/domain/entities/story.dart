class Story {
  const Story({
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

  bool get isVideo => mediaType == 'video';
}
