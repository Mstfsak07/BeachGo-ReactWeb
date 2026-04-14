import 'package:beachgo/features/beach/domain/entities/beach_filter.dart';

class BeachFilterDto {
  const BeachFilterDto({
    required this.hasWifi,
    required this.hasBar,
    required this.minRating,
    required this.hasWaterSports,
    required this.isChildFriendly,
    required this.hasPool,
    required this.freeEntry,
    required this.sortBy,
  });

  final bool? hasWifi;
  final bool? hasBar;
  final double? minRating;
  final bool? hasWaterSports;
  final bool? isChildFriendly;
  final bool? hasPool;
  final bool? freeEntry;
  final String sortBy;

  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      if (hasWifi != null) 'hasWifi': hasWifi,
      if (hasBar != null) 'hasBar': hasBar,
      if (minRating != null) 'minRating': minRating,
      if (hasWaterSports != null) 'hasWaterSports': hasWaterSports,
      if (isChildFriendly != null) 'isChildFriendly': isChildFriendly,
      if (hasPool != null) 'hasPool': hasPool,
      if (freeEntry != null) 'freeEntry': freeEntry,
      'sortBy': sortBy,
    };
  }
}

extension BeachFilterMapper on BeachFilter {
  BeachFilterDto toDto() {
    return BeachFilterDto(
      hasWifi: hasWifi,
      hasBar: hasBar,
      minRating: minRating,
      hasWaterSports: hasWaterSports,
      isChildFriendly: isChildFriendly,
      hasPool: hasPool,
      freeEntry: freeEntry,
      sortBy: sortBy,
    );
  }
}
