class CreateReservationRequestDto {
  const CreateReservationRequestDto({
    required this.beachId,
    required this.reservationDate,
    required this.reservationTime,
    required this.personCount,
    required this.sunbedCount,
    required this.notes,
    required this.totalPrice,
  });

  final int beachId;
  final DateTime reservationDate;
  final String reservationTime;
  final int personCount;
  final int sunbedCount;
  final String notes;
  final double totalPrice;

  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'beachId': beachId,
      'reservationDate': reservationDate.toUtc().toIso8601String(),
      'reservationTime': reservationTime,
      'personCount': personCount,
      'sunbedCount': sunbedCount,
      'notes': notes,
      'totalPrice': totalPrice,
    };
  }
}
