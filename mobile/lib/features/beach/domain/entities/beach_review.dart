class BeachReview {
  const BeachReview({
    required this.userName,
    required this.rating,
    required this.comment,
    required this.createdAt,
  });

  final String userName;
  final int rating;
  final String comment;
  final DateTime createdAt;
}
