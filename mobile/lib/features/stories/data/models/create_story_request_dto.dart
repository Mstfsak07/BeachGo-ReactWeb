class CreateStoryRequestDto {
  const CreateStoryRequestDto({
    required this.beachId,
    required this.mediaUrl,
    required this.mediaType,
    required this.caption,
    required this.expireHours,
  });

  final int beachId;
  final String mediaUrl;
  final String mediaType;
  final String caption;
  final int expireHours;

  Map<String, dynamic> toJson() {
    return {
      'beachId': beachId,
      'mediaUrl': mediaUrl,
      'mediaType': mediaType,
      if (caption.trim().isNotEmpty) 'caption': caption.trim(),
      'expireHours': expireHours,
    };
  }
}
