import 'package:beachgo/features/beach/domain/entities/beach_review.dart';

class BeachReviewDto {
  const BeachReviewDto({
    required this.userName,
    required this.rating,
    required this.comment,
    required this.createdAt,
  });

  final String userName;
  final int rating;
  final String comment;
  final DateTime createdAt;

  factory BeachReviewDto.fromJson(Map<String, dynamic> json) {
    return BeachReviewDto(
      userName: (json['userName'] as String? ?? '').trim(),
      rating: (json['rating'] as num?)?.toInt() ?? 0,
      comment: (json['comment'] as String? ?? '').trim(),
      createdAt: DateTime.tryParse(json['createdAt'] as String? ?? '') ??
          DateTime.fromMillisecondsSinceEpoch(0),
    );
  }

  BeachReview toDomain() {
    return BeachReview(
      userName: userName.isEmpty ? 'Misafir' : userName,
      rating: rating,
      comment: comment,
      createdAt: createdAt,
    );
  }
}
