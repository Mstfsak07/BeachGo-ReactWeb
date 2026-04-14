class Beach {
  const Beach({
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
}
