import 'package:beachgo/features/beach/domain/entities/beach.dart';

class BeachDto {
  const BeachDto({
    required this.id,
    required this.name,
    required this.location,
    required this.address,
    required this.imageUrl,
    required this.entryFee,
    required this.rating,
    required this.reviewCount,
    required this.occupancyPercent,
    required this.openTime,
    required this.closeTime,
    required this.capacity,
    required this.facilities,
    required this.latitude,
    required this.longitude,
    required this.description,
    required this.hasEntryFee,
    required this.isOpen,
    required this.sunbedPrice,
    required this.phone,
    required this.website,
    required this.instagram,
    required this.hasBar,
    required this.hasWaterSports,
    required this.isChildFriendly,
    required this.hasPool,
    required this.hasRestaurant,
    required this.hasWifi,
    required this.hasParking,
    required this.hasSunbeds,
    required this.hasShower,
    required this.hasDJ,
  });

  final int id;
  final String name;
  final String location;
  final String address;
  final String imageUrl;
  final double entryFee;
  final double rating;
  final int reviewCount;
  final double occupancyPercent;
  final String openTime;
  final String closeTime;
  final int capacity;
  final List<String> facilities;
  final double? latitude;
  final double? longitude;
  final String description;
  final bool hasEntryFee;
  final bool isOpen;
  final double sunbedPrice;
  final String phone;
  final String website;
  final String instagram;
  final bool hasBar;
  final bool hasWaterSports;
  final bool isChildFriendly;
  final bool hasPool;
  final bool hasRestaurant;
  final bool hasWifi;
  final bool hasParking;
  final bool hasSunbeds;
  final bool hasShower;
  final bool hasDJ;

  factory BeachDto.fromJson(Map<String, dynamic> json) {
    return BeachDto(
      id: _asInt(json['id']) ?? 0,
      name: _asString(json['name']),
      location: _asString(json['location']),
      address: _asString(json['address']),
      imageUrl: _asString(json['imageUrl']),
      entryFee: _asDouble(json['entryFee']) ?? 0,
      rating: _asDouble(json['rating']) ?? 0,
      reviewCount: _asInt(json['reviewCount']) ?? 0,
      occupancyPercent: _asDouble(json['occupancyPercent']) ?? 0,
      openTime: _asString(json['openTime']),
      closeTime: _asString(json['closeTime']),
      capacity: _asInt(json['capacity']) ?? 0,
      facilities: _asStringList(json['facilities']),
      latitude: _asDouble(json['latitude']),
      longitude: _asDouble(json['longitude']),
      description: _asString(json['description']),
      hasEntryFee: _asBool(json['hasEntryFee']),
      isOpen: _asBool(json['isOpen']),
      sunbedPrice: _asDouble(json['sunbedPrice']) ?? 0,
      phone: _asString(json['phone']),
      website: _asString(json['website']),
      instagram: _asString(json['instagram']),
      hasBar: _asBool(json['hasBar']),
      hasWaterSports: _asBool(json['hasWaterSports']),
      isChildFriendly: _asBool(json['isChildFriendly']),
      hasPool: _asBool(json['hasPool']),
      hasRestaurant: _asBool(json['hasRestaurant']),
      hasWifi: _asBool(json['hasWifi']),
      hasParking: _asBool(json['hasParking']),
      hasSunbeds: _asBool(json['hasSunbeds']),
      hasShower: _asBool(json['hasShower']),
      hasDJ: _asBool(json['hasDJ']),
    );
  }

  static int? _asInt(Object? value) {
    if (value is int) return value;
    if (value is num) return value.toInt();
    return int.tryParse(value?.toString() ?? '');
  }

  static double? _asDouble(Object? value) {
    if (value is double) return value;
    if (value is num) return value.toDouble();
    return double.tryParse(value?.toString() ?? '');
  }

  static bool _asBool(Object? value) {
    if (value is bool) return value;
    if (value is num) return value != 0;
    final normalized = value?.toString().toLowerCase();
    return normalized == 'true' || normalized == '1';
  }

  static String _asString(Object? value) => value?.toString() ?? '';

  static List<String> _asStringList(Object? value) {
    if (value is List) {
      return value.map((item) => item?.toString() ?? '').toList(growable: false);
    }
    return const <String>[];
  }
}

extension BeachDtoMapper on BeachDto {
  Beach toDomain() {
    return Beach(
      id: id,
      name: name,
      location: location,
      address: address,
      imageUrl: imageUrl,
      entryFee: entryFee,
      rating: rating,
      reviewCount: reviewCount,
      occupancyPercent: occupancyPercent,
      openTime: openTime,
      closeTime: closeTime,
      capacity: capacity,
      facilities: facilities,
      latitude: latitude,
      longitude: longitude,
      description: description,
      hasEntryFee: hasEntryFee,
      isOpen: isOpen,
      sunbedPrice: sunbedPrice,
      phone: phone,
      website: website,
      instagram: instagram,
      hasBar: hasBar,
      hasWaterSports: hasWaterSports,
      isChildFriendly: isChildFriendly,
      hasPool: hasPool,
      hasRestaurant: hasRestaurant,
      hasWifi: hasWifi,
      hasParking: hasParking,
      hasSunbeds: hasSunbeds,
      hasShower: hasShower,
      hasDJ: hasDJ,
    );
  }
}
