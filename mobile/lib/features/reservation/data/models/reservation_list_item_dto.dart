import 'package:beachgo/features/reservation/domain/entities/reservation_list_item.dart';

class ReservationListItemDto {
  const ReservationListItemDto({
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

  factory ReservationListItemDto.fromJson(Map<String, dynamic> json) {
    return ReservationListItemDto(
      id: (json['id'] as num?)?.toInt() ?? 0,
      reservationDate: DateTime.tryParse(
            json['reservationDate']?.toString() ?? '',
          ) ??
          DateTime.fromMillisecondsSinceEpoch(0),
      reservationTime: json['reservationTime']?.toString() ?? '',
      status: json['status']?.toString() ?? '',
      beachId: (json['beachId'] as num?)?.toInt() ?? 0,
      beachName: json['beachName']?.toString() ?? '',
    );
  }

  ReservationListItem toDomain() {
    return ReservationListItem(
      id: id,
      reservationDate: reservationDate,
      reservationTime: reservationTime,
      status: status,
      beachId: beachId,
      beachName: beachName,
    );
  }
}
