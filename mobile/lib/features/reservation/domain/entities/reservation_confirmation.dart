class ReservationConfirmation {
  const ReservationConfirmation({
    required this.id,
    required this.beachId,
    required this.beachName,
    required this.reservationDate,
    required this.reservationTime,
    required this.status,
  });

  final int id;
  final int beachId;
  final String beachName;
  final DateTime reservationDate;
  final String reservationTime;
  final String status;
}
