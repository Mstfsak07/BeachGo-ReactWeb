// ignore_for_file: invalid_annotation_target

// Web'deki types.ts'nin Dart karşılığı.
// freezed + json_serializable ile immutable, copyWith destekli modeller.
//
// Kod üretmek için: dart run build_runner build --delete-conflicting-outputs

import 'package:freezed_annotation/freezed_annotation.dart';

part 'models.freezed.dart';
part 'models.g.dart';

// ── AppUser ──────────────────────────────────────────────────────────────────

@freezed
class AppUser with _$AppUser {
  const factory AppUser({
    int? id,
    String? email,
    String? role,
    @JsonKey(name: 'accountType') String? accountType,
    String? firstName,
    String? lastName,
    String? name,
    String? phone,
  }) = _AppUser;

  factory AppUser.fromJson(Map<String, dynamic> json) =>
      _$AppUserFromJson(json);
}

extension AppUserX on AppUser {
  String get displayName {
    if (name != null && name!.isNotEmpty) return name!;
    final parts = [firstName, lastName].whereType<String>().join(' ').trim();
    if (parts.isNotEmpty) return parts;
    return email?.split('@').first ?? 'Kullanıcı';
  }

  bool get isBusiness => role == 'Business' || role == 'Admin';
  bool get isAdmin => role == 'Admin';
}

// ── BeachDto ─────────────────────────────────────────────────────────────────

@freezed
class BeachDto with _$BeachDto {
  const factory BeachDto({
    int? id,
    String? name,
    String? location,
    String? address,
    String? imageUrl,
    double? entryFee,
    double? rating,
    int? reviewCount,
    double? occupancyPercent,
    String? openTime,
    String? closeTime,
    int? capacity,
    List<String>? facilities,
    double? latitude,
    double? longitude,
    String? description,
    bool? hasEntryFee,
    bool? isOpen,
    double? sunbedPrice,
    String? phone,
    String? website,
    String? instagram,
    bool? hasBar,
    bool? hasWaterSports,
    bool? isChildFriendly,
    bool? hasPool,
    bool? hasRestaurant,
    bool? hasWifi,
    bool? hasParking,
    bool? hasSunbeds,
    bool? hasShower,
    bool? hasDJ,
  }) = _BeachDto;

  factory BeachDto.fromJson(Map<String, dynamic> json) =>
      _$BeachDtoFromJson(json);
}

// ── ReservationDto ───────────────────────────────────────────────────────────

@freezed
class ReservationDto with _$ReservationDto {
  const factory ReservationDto({
    int? id,
    String? status,
    String? paymentStatus,
    String? reservationDate,
    String? confirmationCode,
    double? totalPrice,
    String? paymentUrl,
    String? transactionId,
    String? beachName,
    String? customerName,
    String? reservationTime,
    String? reservationType,
    int? pax,
  }) = _ReservationDto;

  factory ReservationDto.fromJson(Map<String, dynamic> json) =>
      _$ReservationDtoFromJson(json);
}

// ── EventDto ─────────────────────────────────────────────────────────────────

@freezed
class EventDto with _$EventDto {
  const factory EventDto({
    dynamic id,
    String? startDate,
    String? date,
    String? title,
    String? imageUrl,
    bool? isActive,
    String? description,
    String? beachName,
    int? beachId,
  }) = _EventDto;

  factory EventDto.fromJson(Map<String, dynamic> json) =>
      _$EventDtoFromJson(json);
}

// ── BeachReviewDto ───────────────────────────────────────────────────────────

@freezed
class BeachReviewDto with _$BeachReviewDto {
  const factory BeachReviewDto({
    String? userName,
    String? createdAt,
    double? rating,
    String? comment,
  }) = _BeachReviewDto;

  factory BeachReviewDto.fromJson(Map<String, dynamic> json) =>
      _$BeachReviewDtoFromJson(json);
}

// ── FavoriteDto ──────────────────────────────────────────────────────────────

@freezed
class FavoriteDto with _$FavoriteDto {
  const factory FavoriteDto({
    int? id,
    int? beachId,
    BeachDto? beach,
  }) = _FavoriteDto;

  factory FavoriteDto.fromJson(Map<String, dynamic> json) =>
      _$FavoriteDtoFromJson(json);
}

// ── Auth ─────────────────────────────────────────────────────────────────────

@freezed
class AuthResponse with _$AuthResponse {
  const factory AuthResponse({
    String? accessToken,
    String? token,
    String? refreshToken,
    AppUser? user,
    String? role,
    String? accountType,
  }) = _AuthResponse;

  factory AuthResponse.fromJson(Map<String, dynamic> json) =>
      _$AuthResponseFromJson(json);
}

// ── BeachFilter (request body) ───────────────────────────────────────────────

@freezed
class BeachFilter with _$BeachFilter {
  const factory BeachFilter({
    double? minRating,
    bool? hasBar,
    bool? hasWaterSports,
    bool? isChildFriendly,
    bool? hasPool,
    bool? freeEntry,
    String? sortBy,
  }) = _BeachFilter;

  factory BeachFilter.fromJson(Map<String, dynamic> json) =>
      _$BeachFilterFromJson(json);
}
