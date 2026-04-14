// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_$AppUserImpl _$$AppUserImplFromJson(Map<String, dynamic> json) =>
    _$AppUserImpl(
      id: (json['id'] as num?)?.toInt(),
      email: json['email'] as String?,
      role: json['role'] as String?,
      accountType: json['accountType'] as String?,
      firstName: json['firstName'] as String?,
      lastName: json['lastName'] as String?,
      name: json['name'] as String?,
      phone: json['phone'] as String?,
    );

Map<String, dynamic> _$$AppUserImplToJson(_$AppUserImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'email': instance.email,
      'role': instance.role,
      'accountType': instance.accountType,
      'firstName': instance.firstName,
      'lastName': instance.lastName,
      'name': instance.name,
      'phone': instance.phone,
    };

_$BeachDtoImpl _$$BeachDtoImplFromJson(Map<String, dynamic> json) =>
    _$BeachDtoImpl(
      id: (json['id'] as num?)?.toInt(),
      name: json['name'] as String?,
      location: json['location'] as String?,
      address: json['address'] as String?,
      imageUrl: json['imageUrl'] as String?,
      entryFee: (json['entryFee'] as num?)?.toDouble(),
      rating: (json['rating'] as num?)?.toDouble(),
      reviewCount: (json['reviewCount'] as num?)?.toInt(),
      occupancyPercent: (json['occupancyPercent'] as num?)?.toDouble(),
      openTime: json['openTime'] as String?,
      closeTime: json['closeTime'] as String?,
      capacity: (json['capacity'] as num?)?.toInt(),
      facilities: (json['facilities'] as List<dynamic>?)
          ?.map((e) => e as String)
          .toList(),
      latitude: (json['latitude'] as num?)?.toDouble(),
      longitude: (json['longitude'] as num?)?.toDouble(),
      description: json['description'] as String?,
      hasEntryFee: json['hasEntryFee'] as bool?,
      isOpen: json['isOpen'] as bool?,
      sunbedPrice: (json['sunbedPrice'] as num?)?.toDouble(),
      phone: json['phone'] as String?,
      website: json['website'] as String?,
      instagram: json['instagram'] as String?,
      hasBar: json['hasBar'] as bool?,
      hasWaterSports: json['hasWaterSports'] as bool?,
      isChildFriendly: json['isChildFriendly'] as bool?,
      hasPool: json['hasPool'] as bool?,
      hasRestaurant: json['hasRestaurant'] as bool?,
      hasWifi: json['hasWifi'] as bool?,
      hasParking: json['hasParking'] as bool?,
      hasSunbeds: json['hasSunbeds'] as bool?,
      hasShower: json['hasShower'] as bool?,
      hasDJ: json['hasDJ'] as bool?,
    );

Map<String, dynamic> _$$BeachDtoImplToJson(_$BeachDtoImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'location': instance.location,
      'address': instance.address,
      'imageUrl': instance.imageUrl,
      'entryFee': instance.entryFee,
      'rating': instance.rating,
      'reviewCount': instance.reviewCount,
      'occupancyPercent': instance.occupancyPercent,
      'openTime': instance.openTime,
      'closeTime': instance.closeTime,
      'capacity': instance.capacity,
      'facilities': instance.facilities,
      'latitude': instance.latitude,
      'longitude': instance.longitude,
      'description': instance.description,
      'hasEntryFee': instance.hasEntryFee,
      'isOpen': instance.isOpen,
      'sunbedPrice': instance.sunbedPrice,
      'phone': instance.phone,
      'website': instance.website,
      'instagram': instance.instagram,
      'hasBar': instance.hasBar,
      'hasWaterSports': instance.hasWaterSports,
      'isChildFriendly': instance.isChildFriendly,
      'hasPool': instance.hasPool,
      'hasRestaurant': instance.hasRestaurant,
      'hasWifi': instance.hasWifi,
      'hasParking': instance.hasParking,
      'hasSunbeds': instance.hasSunbeds,
      'hasShower': instance.hasShower,
      'hasDJ': instance.hasDJ,
    };

_$ReservationDtoImpl _$$ReservationDtoImplFromJson(Map<String, dynamic> json) =>
    _$ReservationDtoImpl(
      id: (json['id'] as num?)?.toInt(),
      status: json['status'] as String?,
      paymentStatus: json['paymentStatus'] as String?,
      reservationDate: json['reservationDate'] as String?,
      confirmationCode: json['confirmationCode'] as String?,
      totalPrice: (json['totalPrice'] as num?)?.toDouble(),
      paymentUrl: json['paymentUrl'] as String?,
      transactionId: json['transactionId'] as String?,
      beachName: json['beachName'] as String?,
      customerName: json['customerName'] as String?,
      reservationTime: json['reservationTime'] as String?,
      reservationType: json['reservationType'] as String?,
      pax: (json['pax'] as num?)?.toInt(),
    );

Map<String, dynamic> _$$ReservationDtoImplToJson(
        _$ReservationDtoImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'status': instance.status,
      'paymentStatus': instance.paymentStatus,
      'reservationDate': instance.reservationDate,
      'confirmationCode': instance.confirmationCode,
      'totalPrice': instance.totalPrice,
      'paymentUrl': instance.paymentUrl,
      'transactionId': instance.transactionId,
      'beachName': instance.beachName,
      'customerName': instance.customerName,
      'reservationTime': instance.reservationTime,
      'reservationType': instance.reservationType,
      'pax': instance.pax,
    };

_$EventDtoImpl _$$EventDtoImplFromJson(Map<String, dynamic> json) =>
    _$EventDtoImpl(
      id: json['id'],
      startDate: json['startDate'] as String?,
      date: json['date'] as String?,
      title: json['title'] as String?,
      imageUrl: json['imageUrl'] as String?,
      isActive: json['isActive'] as bool?,
      description: json['description'] as String?,
      beachName: json['beachName'] as String?,
      beachId: (json['beachId'] as num?)?.toInt(),
    );

Map<String, dynamic> _$$EventDtoImplToJson(_$EventDtoImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'startDate': instance.startDate,
      'date': instance.date,
      'title': instance.title,
      'imageUrl': instance.imageUrl,
      'isActive': instance.isActive,
      'description': instance.description,
      'beachName': instance.beachName,
      'beachId': instance.beachId,
    };

_$BeachReviewDtoImpl _$$BeachReviewDtoImplFromJson(Map<String, dynamic> json) =>
    _$BeachReviewDtoImpl(
      userName: json['userName'] as String?,
      createdAt: json['createdAt'] as String?,
      rating: (json['rating'] as num?)?.toDouble(),
      comment: json['comment'] as String?,
    );

Map<String, dynamic> _$$BeachReviewDtoImplToJson(
        _$BeachReviewDtoImpl instance) =>
    <String, dynamic>{
      'userName': instance.userName,
      'createdAt': instance.createdAt,
      'rating': instance.rating,
      'comment': instance.comment,
    };

_$FavoriteDtoImpl _$$FavoriteDtoImplFromJson(Map<String, dynamic> json) =>
    _$FavoriteDtoImpl(
      id: (json['id'] as num?)?.toInt(),
      beachId: (json['beachId'] as num?)?.toInt(),
      beach: json['beach'] == null
          ? null
          : BeachDto.fromJson(json['beach'] as Map<String, dynamic>),
    );

Map<String, dynamic> _$$FavoriteDtoImplToJson(_$FavoriteDtoImpl instance) =>
    <String, dynamic>{
      'id': instance.id,
      'beachId': instance.beachId,
      'beach': instance.beach,
    };

_$AuthResponseImpl _$$AuthResponseImplFromJson(Map<String, dynamic> json) =>
    _$AuthResponseImpl(
      accessToken: json['accessToken'] as String?,
      token: json['token'] as String?,
      refreshToken: json['refreshToken'] as String?,
      user: json['user'] == null
          ? null
          : AppUser.fromJson(json['user'] as Map<String, dynamic>),
      role: json['role'] as String?,
      accountType: json['accountType'] as String?,
    );

Map<String, dynamic> _$$AuthResponseImplToJson(_$AuthResponseImpl instance) =>
    <String, dynamic>{
      'accessToken': instance.accessToken,
      'token': instance.token,
      'refreshToken': instance.refreshToken,
      'user': instance.user,
      'role': instance.role,
      'accountType': instance.accountType,
    };

_$BeachFilterImpl _$$BeachFilterImplFromJson(Map<String, dynamic> json) =>
    _$BeachFilterImpl(
      minRating: (json['minRating'] as num?)?.toDouble(),
      hasBar: json['hasBar'] as bool?,
      hasWaterSports: json['hasWaterSports'] as bool?,
      isChildFriendly: json['isChildFriendly'] as bool?,
      hasPool: json['hasPool'] as bool?,
      freeEntry: json['freeEntry'] as bool?,
      sortBy: json['sortBy'] as String?,
    );

Map<String, dynamic> _$$BeachFilterImplToJson(_$BeachFilterImpl instance) =>
    <String, dynamic>{
      'minRating': instance.minRating,
      'hasBar': instance.hasBar,
      'hasWaterSports': instance.hasWaterSports,
      'isChildFriendly': instance.isChildFriendly,
      'hasPool': instance.hasPool,
      'freeEntry': instance.freeEntry,
      'sortBy': instance.sortBy,
    };
