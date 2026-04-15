class ReservationListItem {
  const ReservationListItem({
    required this.id,
    required this.reservationDate,
    required this.reservationTime,
    required this.status,
    required this.beachId,
    required this.beachName,
  });

  final int id;
  final DateTime reservationDate;
  final String reservationTime;
  final String status;
  final int beachId;
  final String beachName;
}
