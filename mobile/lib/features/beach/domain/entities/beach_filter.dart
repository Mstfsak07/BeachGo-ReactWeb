class BeachFilter {
  static const _unset = Object();

  const BeachFilter({
    this.hasWifi,
    this.hasBar,
    this.minRating,
    this.hasWaterSports,
    this.isChildFriendly,
    this.hasPool,
    this.freeEntry,
    this.sortBy = 'rating',
  });

  final bool? hasWifi;
  final bool? hasBar;
  final double? minRating;
  final bool? hasWaterSports;
  final bool? isChildFriendly;
  final bool? hasPool;
  final bool? freeEntry;
  final String sortBy;

  BeachFilter copyWith({
    Object? hasWifi = _unset,
    Object? hasBar = _unset,
    Object? minRating = _unset,
    Object? hasWaterSports = _unset,
    Object? isChildFriendly = _unset,
    Object? hasPool = _unset,
    Object? freeEntry = _unset,
    String? sortBy,
  }) {
    return BeachFilter(
      hasWifi: identical(hasWifi, _unset) ? this.hasWifi : hasWifi as bool?,
      hasBar: identical(hasBar, _unset) ? this.hasBar : hasBar as bool?,
      minRating: identical(minRating, _unset)
          ? this.minRating
          : minRating as double?,
      hasWaterSports: identical(hasWaterSports, _unset)
          ? this.hasWaterSports
          : hasWaterSports as bool?,
      isChildFriendly: identical(isChildFriendly, _unset)
          ? this.isChildFriendly
          : isChildFriendly as bool?,
      hasPool: identical(hasPool, _unset) ? this.hasPool : hasPool as bool?,
      freeEntry:
          identical(freeEntry, _unset) ? this.freeEntry : freeEntry as bool?,
      sortBy: sortBy ?? this.sortBy,
    );
  }
}
