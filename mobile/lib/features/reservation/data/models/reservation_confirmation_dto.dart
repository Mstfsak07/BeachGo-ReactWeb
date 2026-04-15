import 'package:beachgo/features/reservation/domain/entities/reservation_confirmation.dart';

class ReservationConfirmationDto {
  const ReservationConfirmationDto({
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

  factory ReservationConfirmationDto.fromJson(Map<String, dynamic> json) {
    return ReservationConfirmationDto(
      id: json['id'] as int? ?? 0,
      beachId: json['beachId'] as int? ?? 0,
      beachName: json['beachName'] as String? ?? '',
      reservationDate: DateTime.tryParse(json['reservationDate'] as String? ?? '')
              ?.toUtc() ??
          DateTime.now().toUtc(),
      reservationTime: json['reservationTime'] as String? ?? '',
      status: json['status'] as String? ?? '',
    );
  }
}

extension ReservationConfirmationDtoMapper on ReservationConfirmationDto {
  ReservationConfirmation toDomain() {
    return ReservationConfirmation(
      id: id,
      beachId: beachId,
      beachName: beachName,
      reservationDate: reservationDate,
      reservationTime: reservationTime,
      status: status,
    );
  }
}
