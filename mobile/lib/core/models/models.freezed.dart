// coverage:ignore-file
// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'models.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

T _$identity<T>(T value) => value;

final _privateConstructorUsedError = UnsupportedError(
    'It seems like you constructed your class using `MyClass._()`. This constructor is only meant to be used by freezed and you are not supposed to need it nor use it.\nPlease check the documentation here for more information: https://github.com/rrousselGit/freezed#adding-getters-and-methods-to-our-models');

AppUser _$AppUserFromJson(Map<String, dynamic> json) {
  return _AppUser.fromJson(json);
}

/// @nodoc
mixin _$AppUser {
  int? get id => throw _privateConstructorUsedError;
  String? get email => throw _privateConstructorUsedError;
  String? get role => throw _privateConstructorUsedError;
  @JsonKey(name: 'accountType')
  String? get accountType => throw _privateConstructorUsedError;
  String? get firstName => throw _privateConstructorUsedError;
  String? get lastName => throw _privateConstructorUsedError;
  String? get name => throw _privateConstructorUsedError;
  String? get phone => throw _privateConstructorUsedError;

  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;
  @JsonKey(ignore: true)
  $AppUserCopyWith<AppUser> get copyWith => throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $AppUserCopyWith<$Res> {
  factory $AppUserCopyWith(AppUser value, $Res Function(AppUser) then) =
      _$AppUserCopyWithImpl<$Res, AppUser>;
  @useResult
  $Res call(
      {int? id,
      String? email,
      String? role,
      @JsonKey(name: 'accountType') String? accountType,
      String? firstName,
      String? lastName,
      String? name,
      String? phone});
}

/// @nodoc
class _$AppUserCopyWithImpl<$Res, $Val extends AppUser>
    implements $AppUserCopyWith<$Res> {
  _$AppUserCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? email = freezed,
    Object? role = freezed,
    Object? accountType = freezed,
    Object? firstName = freezed,
    Object? lastName = freezed,
    Object? name = freezed,
    Object? phone = freezed,
  }) {
    return _then(_value.copyWith(
      id: freezed == id
          ? _value.id
          : id // ignore: cast_nullable_to_non_nullable
              as int?,
      email: freezed == email
          ? _value.email
          : email // ignore: cast_nullable_to_non_nullable
              as String?,
      role: freezed == role
          ? _value.role
          : role // ignore: cast_nullable_to_non_nullable
              as String?,
      accountType: freezed == accountType
          ? _value.accountType
          : accountType // ignore: cast_nullable_to_non_nullable
              as String?,
      firstName: freezed == firstName
          ? _value.firstName
          : firstName // ignore: cast_nullable_to_non_nullable
              as String?,
      lastName: freezed == lastName
          ? _value.lastName
          : lastName // ignore: cast_nullable_to_non_nullable
              as String?,
      name: freezed == name
          ? _value.name
          : name // ignore: cast_nullable_to_non_nullable
              as String?,
      phone: freezed == phone
          ? _value.phone
          : phone // ignore: cast_nullable_to_non_nullable
              as String?,
    ) as $Val);
  }
}

/// @nodoc
abstract class _$$AppUserImplCopyWith<$Res> implements $AppUserCopyWith<$Res> {
  factory _$$AppUserImplCopyWith(
          _$AppUserImpl value, $Res Function(_$AppUserImpl) then) =
      __$$AppUserImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call(
      {int? id,
      String? email,
      String? role,
      @JsonKey(name: 'accountType') String? accountType,
      String? firstName,
      String? lastName,
      String? name,
      String? phone});
}

/// @nodoc
class __$$AppUserImplCopyWithImpl<$Res>
    extends _$AppUserCopyWithImpl<$Res, _$AppUserImpl>
    implements _$$AppUserImplCopyWith<$Res> {
  __$$AppUserImplCopyWithImpl(
      _$AppUserImpl _value, $Res Function(_$AppUserImpl) _then)
      : super(_value, _then);

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? email = freezed,
    Object? role = freezed,
    Object? accountType = freezed,
    Object? firstName = freezed,
    Object? lastName = freezed,
    Object? name = freezed,
    Object? phone = freezed,
  }) {
    return _then(_$AppUserImpl(
      id: freezed == id
          ? _value.id
          : id // ignore: cast_nullable_to_non_nullable
              as int?,
      email: freezed == email
          ? _value.email
          : email // ignore: cast_nullable_to_non_nullable
              as String?,
      role: freezed == role
          ? _value.role
          : role // ignore: cast_nullable_to_non_nullable
              as String?,
      accountType: freezed == accountType
          ? _value.accountType
          : accountType // ignore: cast_nullable_to_non_nullable
              as String?,
      firstName: freezed == firstName
          ? _value.firstName
          : firstName // ignore: cast_nullable_to_non_nullable
              as String?,
      lastName: freezed == lastName
          ? _value.lastName
          : lastName // ignore: cast_nullable_to_non_nullable
              as String?,
      name: freezed == name
          ? _value.name
          : name // ignore: cast_nullable_to_non_nullable
              as String?,
      phone: freezed == phone
          ? _value.phone
          : phone // ignore: cast_nullable_to_non_nullable
              as String?,
    ));
  }
}

/// @nodoc
@JsonSerializable()
class _$AppUserImpl implements _AppUser {
  const _$AppUserImpl(
      {this.id,
      this.email,
      this.role,
      @JsonKey(name: 'accountType') this.accountType,
      this.firstName,
      this.lastName,
      this.name,
      this.phone});

  factory _$AppUserImpl.fromJson(Map<String, dynamic> json) =>
      _$$AppUserImplFromJson(json);

  @override
  final int? id;
  @override
  final String? email;
  @override
  final String? role;
  @override
  @JsonKey(name: 'accountType')
  final String? accountType;
  @override
  final String? firstName;
  @override
  final String? lastName;
  @override
  final String? name;
  @override
  final String? phone;

  @override
  String toString() {
    return 'AppUser(id: $id, email: $email, role: $role, accountType: $accountType, firstName: $firstName, lastName: $lastName, name: $name, phone: $phone)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$AppUserImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.email, email) || other.email == email) &&
            (identical(other.role, role) || other.role == role) &&
            (identical(other.accountType, accountType) ||
                other.accountType == accountType) &&
            (identical(other.firstName, firstName) ||
                other.firstName == firstName) &&
            (identical(other.lastName, lastName) ||
                other.lastName == lastName) &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.phone, phone) || other.phone == phone));
  }

  @JsonKey(ignore: true)
  @override
  int get hashCode => Object.hash(runtimeType, id, email, role, accountType,
      firstName, lastName, name, phone);

  @JsonKey(ignore: true)
  @override
  @pragma('vm:prefer-inline')
  _$$AppUserImplCopyWith<_$AppUserImpl> get copyWith =>
      __$$AppUserImplCopyWithImpl<_$AppUserImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$AppUserImplToJson(
      this,
    );
  }
}

abstract class _AppUser implements AppUser {
  const factory _AppUser(
      {final int? id,
      final String? email,
      final String? role,
      @JsonKey(name: 'accountType') final String? accountType,
      final String? firstName,
      final String? lastName,
      final String? name,
      final String? phone}) = _$AppUserImpl;

  factory _AppUser.fromJson(Map<String, dynamic> json) = _$AppUserImpl.fromJson;

  @override
  int? get id;
  @override
  String? get email;
  @override
  String? get role;
  @override
  @JsonKey(name: 'accountType')
  String? get accountType;
  @override
  String? get firstName;
  @override
  String? get lastName;
  @override
  String? get name;
  @override
  String? get phone;
  @override
  @JsonKey(ignore: true)
  _$$AppUserImplCopyWith<_$AppUserImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

BeachDto _$BeachDtoFromJson(Map<String, dynamic> json) {
  return _BeachDto.fromJson(json);
}

/// @nodoc
mixin _$BeachDto {
  int? get id => throw _privateConstructorUsedError;
  String? get name => throw _privateConstructorUsedError;
  String? get location => throw _privateConstructorUsedError;
  String? get address => throw _privateConstructorUsedError;
  String? get imageUrl => throw _privateConstructorUsedError;
  double? get entryFee => throw _privateConstructorUsedError;
  double? get rating => throw _privateConstructorUsedError;
  int? get reviewCount => throw _privateConstructorUsedError;
  double? get occupancyPercent => throw _privateConstructorUsedError;
  String? get openTime => throw _privateConstructorUsedError;
  String? get closeTime => throw _privateConstructorUsedError;
  int? get capacity => throw _privateConstructorUsedError;
  List<String>? get facilities => throw _privateConstructorUsedError;
  double? get latitude => throw _privateConstructorUsedError;
  double? get longitude => throw _privateConstructorUsedError;
  String? get description => throw _privateConstructorUsedError;
  bool? get hasEntryFee => throw _privateConstructorUsedError;
  bool? get isOpen => throw _privateConstructorUsedError;
  double? get sunbedPrice => throw _privateConstructorUsedError;
  String? get phone => throw _privateConstructorUsedError;
  String? get website => throw _privateConstructorUsedError;
  String? get instagram => throw _privateConstructorUsedError;
  bool? get hasBar => throw _privateConstructorUsedError;
  bool? get hasWaterSports => throw _privateConstructorUsedError;
  bool? get isChildFriendly => throw _privateConstructorUsedError;
  bool? get hasPool => throw _privateConstructorUsedError;
  bool? get hasRestaurant => throw _privateConstructorUsedError;
  bool? get hasWifi => throw _privateConstructorUsedError;
  bool? get hasParking => throw _privateConstructorUsedError;
  bool? get hasSunbeds => throw _privateConstructorUsedError;
  bool? get hasShower => throw _privateConstructorUsedError;
  bool? get hasDJ => throw _privateConstructorUsedError;

  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;
  @JsonKey(ignore: true)
  $BeachDtoCopyWith<BeachDto> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $BeachDtoCopyWith<$Res> {
  factory $BeachDtoCopyWith(BeachDto value, $Res Function(BeachDto) then) =
      _$BeachDtoCopyWithImpl<$Res, BeachDto>;
  @useResult
  $Res call(
      {int? id,
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
      bool? hasDJ});
}

/// @nodoc
class _$BeachDtoCopyWithImpl<$Res, $Val extends BeachDto>
    implements $BeachDtoCopyWith<$Res> {
  _$BeachDtoCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? name = freezed,
    Object? location = freezed,
    Object? address = freezed,
    Object? imageUrl = freezed,
    Object? entryFee = freezed,
    Object? rating = freezed,
    Object? reviewCount = freezed,
    Object? occupancyPercent = freezed,
    Object? openTime = freezed,
    Object? closeTime = freezed,
    Object? capacity = freezed,
    Object? facilities = freezed,
    Object? latitude = freezed,
    Object? longitude = freezed,
    Object? description = freezed,
    Object? hasEntryFee = freezed,
    Object? isOpen = freezed,
    Object? sunbedPrice = freezed,
    Object? phone = freezed,
    Object? website = freezed,
    Object? instagram = freezed,
    Object? hasBar = freezed,
    Object? hasWaterSports = freezed,
    Object? isChildFriendly = freezed,
    Object? hasPool = freezed,
    Object? hasRestaurant = freezed,
    Object? hasWifi = freezed,
    Object? hasParking = freezed,
    Object? hasSunbeds = freezed,
    Object? hasShower = freezed,
    Object? hasDJ = freezed,
  }) {
    return _then(_value.copyWith(
      id: freezed == id
          ? _value.id
          : id // ignore: cast_nullable_to_non_nullable
              as int?,
      name: freezed == name
          ? _value.name
          : name // ignore: cast_nullable_to_non_nullable
              as String?,
      location: freezed == location
          ? _value.location
          : location // ignore: cast_nullable_to_non_nullable
              as String?,
      address: freezed == address
          ? _value.address
          : address // ignore: cast_nullable_to_non_nullable
              as String?,
      imageUrl: freezed == imageUrl
          ? _value.imageUrl
          : imageUrl // ignore: cast_nullable_to_non_nullable
              as String?,
      entryFee: freezed == entryFee
          ? _value.entryFee
          : entryFee // ignore: cast_nullable_to_non_nullable
              as double?,
      rating: freezed == rating
          ? _value.rating
          : rating // ignore: cast_nullable_to_non_nullable
              as double?,
      reviewCount: freezed == reviewCount
          ? _value.reviewCount
          : reviewCount // ignore: cast_nullable_to_non_nullable
              as int?,
      occupancyPercent: freezed == occupancyPercent
          ? _value.occupancyPercent
          : occupancyPercent // ignore: cast_nullable_to_non_nullable
              as double?,
      openTime: freezed == openTime
          ? _value.openTime
          : openTime // ignore: cast_nullable_to_non_nullable
              as String?,
      closeTime: freezed == closeTime
          ? _value.closeTime
          : closeTime // ignore: cast_nullable_to_non_nullable
              as String?,
      capacity: freezed == capacity
          ? _value.capacity
          : capacity // ignore: cast_nullable_to_non_nullable
              as int?,
      facilities: freezed == facilities
          ? _value.facilities
          : facilities // ignore: cast_nullable_to_non_nullable
              as List<String>?,
      latitude: freezed == latitude
          ? _value.latitude
          : latitude // ignore: cast_nullable_to_non_nullable
              as double?,
      longitude: freezed == longitude
          ? _value.longitude
          : longitude // ignore: cast_nullable_to_non_nullable
              as double?,
      description: freezed == description
          ? _value.description
          : description // ignore: cast_nullable_to_non_nullable
              as String?,
      hasEntryFee: freezed == hasEntryFee
          ? _value.hasEntryFee
          : hasEntryFee // ignore: cast_nullable_to_non_nullable
              as bool?,
      isOpen: freezed == isOpen
          ? _value.isOpen
          : isOpen // ignore: cast_nullable_to_non_nullable
              as bool?,
      sunbedPrice: freezed == sunbedPrice
          ? _value.sunbedPrice
          : sunbedPrice // ignore: cast_nullable_to_non_nullable
              as double?,
      phone: freezed == phone
          ? _value.phone
          : phone // ignore: cast_nullable_to_non_nullable
              as String?,
      website: freezed == website
          ? _value.website
          : website // ignore: cast_nullable_to_non_nullable
              as String?,
      instagram: freezed == instagram
          ? _value.instagram
          : instagram // ignore: cast_nullable_to_non_nullable
              as String?,
      hasBar: freezed == hasBar
          ? _value.hasBar
          : hasBar // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasWaterSports: freezed == hasWaterSports
          ? _value.hasWaterSports
          : hasWaterSports // ignore: cast_nullable_to_non_nullable
              as bool?,
      isChildFriendly: freezed == isChildFriendly
          ? _value.isChildFriendly
          : isChildFriendly // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasPool: freezed == hasPool
          ? _value.hasPool
          : hasPool // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasRestaurant: freezed == hasRestaurant
          ? _value.hasRestaurant
          : hasRestaurant // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasWifi: freezed == hasWifi
          ? _value.hasWifi
          : hasWifi // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasParking: freezed == hasParking
          ? _value.hasParking
          : hasParking // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasSunbeds: freezed == hasSunbeds
          ? _value.hasSunbeds
          : hasSunbeds // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasShower: freezed == hasShower
          ? _value.hasShower
          : hasShower // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasDJ: freezed == hasDJ
          ? _value.hasDJ
          : hasDJ // ignore: cast_nullable_to_non_nullable
              as bool?,
    ) as $Val);
  }
}

/// @nodoc
abstract class _$$BeachDtoImplCopyWith<$Res>
    implements $BeachDtoCopyWith<$Res> {
  factory _$$BeachDtoImplCopyWith(
          _$BeachDtoImpl value, $Res Function(_$BeachDtoImpl) then) =
      __$$BeachDtoImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call(
      {int? id,
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
      bool? hasDJ});
}

/// @nodoc
class __$$BeachDtoImplCopyWithImpl<$Res>
    extends _$BeachDtoCopyWithImpl<$Res, _$BeachDtoImpl>
    implements _$$BeachDtoImplCopyWith<$Res> {
  __$$BeachDtoImplCopyWithImpl(
      _$BeachDtoImpl _value, $Res Function(_$BeachDtoImpl) _then)
      : super(_value, _then);

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? name = freezed,
    Object? location = freezed,
    Object? address = freezed,
    Object? imageUrl = freezed,
    Object? entryFee = freezed,
    Object? rating = freezed,
    Object? reviewCount = freezed,
    Object? occupancyPercent = freezed,
    Object? openTime = freezed,
    Object? closeTime = freezed,
    Object? capacity = freezed,
    Object? facilities = freezed,
    Object? latitude = freezed,
    Object? longitude = freezed,
    Object? description = freezed,
    Object? hasEntryFee = freezed,
    Object? isOpen = freezed,
    Object? sunbedPrice = freezed,
    Object? phone = freezed,
    Object? website = freezed,
    Object? instagram = freezed,
    Object? hasBar = freezed,
    Object? hasWaterSports = freezed,
    Object? isChildFriendly = freezed,
    Object? hasPool = freezed,
    Object? hasRestaurant = freezed,
    Object? hasWifi = freezed,
    Object? hasParking = freezed,
    Object? hasSunbeds = freezed,
    Object? hasShower = freezed,
    Object? hasDJ = freezed,
  }) {
    return _then(_$BeachDtoImpl(
      id: freezed == id
          ? _value.id
          : id // ignore: cast_nullable_to_non_nullable
              as int?,
      name: freezed == name
          ? _value.name
          : name // ignore: cast_nullable_to_non_nullable
              as String?,
      location: freezed == location
          ? _value.location
          : location // ignore: cast_nullable_to_non_nullable
              as String?,
      address: freezed == address
          ? _value.address
          : address // ignore: cast_nullable_to_non_nullable
              as String?,
      imageUrl: freezed == imageUrl
          ? _value.imageUrl
          : imageUrl // ignore: cast_nullable_to_non_nullable
              as String?,
      entryFee: freezed == entryFee
          ? _value.entryFee
          : entryFee // ignore: cast_nullable_to_non_nullable
              as double?,
      rating: freezed == rating
          ? _value.rating
          : rating // ignore: cast_nullable_to_non_nullable
              as double?,
      reviewCount: freezed == reviewCount
          ? _value.reviewCount
          : reviewCount // ignore: cast_nullable_to_non_nullable
              as int?,
      occupancyPercent: freezed == occupancyPercent
          ? _value.occupancyPercent
          : occupancyPercent // ignore: cast_nullable_to_non_nullable
              as double?,
      openTime: freezed == openTime
          ? _value.openTime
          : openTime // ignore: cast_nullable_to_non_nullable
              as String?,
      closeTime: freezed == closeTime
          ? _value.closeTime
          : closeTime // ignore: cast_nullable_to_non_nullable
              as String?,
      capacity: freezed == capacity
          ? _value.capacity
          : capacity // ignore: cast_nullable_to_non_nullable
              as int?,
      facilities: freezed == facilities
          ? _value._facilities
          : facilities // ignore: cast_nullable_to_non_nullable
              as List<String>?,
      latitude: freezed == latitude
          ? _value.latitude
          : latitude // ignore: cast_nullable_to_non_nullable
              as double?,
      longitude: freezed == longitude
          ? _value.longitude
          : longitude // ignore: cast_nullable_to_non_nullable
              as double?,
      description: freezed == description
          ? _value.description
          : description // ignore: cast_nullable_to_non_nullable
              as String?,
      hasEntryFee: freezed == hasEntryFee
          ? _value.hasEntryFee
          : hasEntryFee // ignore: cast_nullable_to_non_nullable
              as bool?,
      isOpen: freezed == isOpen
          ? _value.isOpen
          : isOpen // ignore: cast_nullable_to_non_nullable
              as bool?,
      sunbedPrice: freezed == sunbedPrice
          ? _value.sunbedPrice
          : sunbedPrice // ignore: cast_nullable_to_non_nullable
              as double?,
      phone: freezed == phone
          ? _value.phone
          : phone // ignore: cast_nullable_to_non_nullable
              as String?,
      website: freezed == website
          ? _value.website
          : website // ignore: cast_nullable_to_non_nullable
              as String?,
      instagram: freezed == instagram
          ? _value.instagram
          : instagram // ignore: cast_nullable_to_non_nullable
              as String?,
      hasBar: freezed == hasBar
          ? _value.hasBar
          : hasBar // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasWaterSports: freezed == hasWaterSports
          ? _value.hasWaterSports
          : hasWaterSports // ignore: cast_nullable_to_non_nullable
              as bool?,
      isChildFriendly: freezed == isChildFriendly
          ? _value.isChildFriendly
          : isChildFriendly // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasPool: freezed == hasPool
          ? _value.hasPool
          : hasPool // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasRestaurant: freezed == hasRestaurant
          ? _value.hasRestaurant
          : hasRestaurant // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasWifi: freezed == hasWifi
          ? _value.hasWifi
          : hasWifi // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasParking: freezed == hasParking
          ? _value.hasParking
          : hasParking // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasSunbeds: freezed == hasSunbeds
          ? _value.hasSunbeds
          : hasSunbeds // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasShower: freezed == hasShower
          ? _value.hasShower
          : hasShower // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasDJ: freezed == hasDJ
          ? _value.hasDJ
          : hasDJ // ignore: cast_nullable_to_non_nullable
              as bool?,
    ));
  }
}

/// @nodoc
@JsonSerializable()
class _$BeachDtoImpl implements _BeachDto {
  const _$BeachDtoImpl(
      {this.id,
      this.name,
      this.location,
      this.address,
      this.imageUrl,
      this.entryFee,
      this.rating,
      this.reviewCount,
      this.occupancyPercent,
      this.openTime,
      this.closeTime,
      this.capacity,
      final List<String>? facilities,
      this.latitude,
      this.longitude,
      this.description,
      this.hasEntryFee,
      this.isOpen,
      this.sunbedPrice,
      this.phone,
      this.website,
      this.instagram,
      this.hasBar,
      this.hasWaterSports,
      this.isChildFriendly,
      this.hasPool,
      this.hasRestaurant,
      this.hasWifi,
      this.hasParking,
      this.hasSunbeds,
      this.hasShower,
      this.hasDJ})
      : _facilities = facilities;

  factory _$BeachDtoImpl.fromJson(Map<String, dynamic> json) =>
      _$$BeachDtoImplFromJson(json);

  @override
  final int? id;
  @override
  final String? name;
  @override
  final String? location;
  @override
  final String? address;
  @override
  final String? imageUrl;
  @override
  final double? entryFee;
  @override
  final double? rating;
  @override
  final int? reviewCount;
  @override
  final double? occupancyPercent;
  @override
  final String? openTime;
  @override
  final String? closeTime;
  @override
  final int? capacity;
  final List<String>? _facilities;
  @override
  List<String>? get facilities {
    final value = _facilities;
    if (value == null) return null;
    if (_facilities is EqualUnmodifiableListView) return _facilities;
    // ignore: implicit_dynamic_type
    return EqualUnmodifiableListView(value);
  }

  @override
  final double? latitude;
  @override
  final double? longitude;
  @override
  final String? description;
  @override
  final bool? hasEntryFee;
  @override
  final bool? isOpen;
  @override
  final double? sunbedPrice;
  @override
  final String? phone;
  @override
  final String? website;
  @override
  final String? instagram;
  @override
  final bool? hasBar;
  @override
  final bool? hasWaterSports;
  @override
  final bool? isChildFriendly;
  @override
  final bool? hasPool;
  @override
  final bool? hasRestaurant;
  @override
  final bool? hasWifi;
  @override
  final bool? hasParking;
  @override
  final bool? hasSunbeds;
  @override
  final bool? hasShower;
  @override
  final bool? hasDJ;

  @override
  String toString() {
    return 'BeachDto(id: $id, name: $name, location: $location, address: $address, imageUrl: $imageUrl, entryFee: $entryFee, rating: $rating, reviewCount: $reviewCount, occupancyPercent: $occupancyPercent, openTime: $openTime, closeTime: $closeTime, capacity: $capacity, facilities: $facilities, latitude: $latitude, longitude: $longitude, description: $description, hasEntryFee: $hasEntryFee, isOpen: $isOpen, sunbedPrice: $sunbedPrice, phone: $phone, website: $website, instagram: $instagram, hasBar: $hasBar, hasWaterSports: $hasWaterSports, isChildFriendly: $isChildFriendly, hasPool: $hasPool, hasRestaurant: $hasRestaurant, hasWifi: $hasWifi, hasParking: $hasParking, hasSunbeds: $hasSunbeds, hasShower: $hasShower, hasDJ: $hasDJ)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$BeachDtoImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.name, name) || other.name == name) &&
            (identical(other.location, location) ||
                other.location == location) &&
            (identical(other.address, address) || other.address == address) &&
            (identical(other.imageUrl, imageUrl) ||
                other.imageUrl == imageUrl) &&
            (identical(other.entryFee, entryFee) ||
                other.entryFee == entryFee) &&
            (identical(other.rating, rating) || other.rating == rating) &&
            (identical(other.reviewCount, reviewCount) ||
                other.reviewCount == reviewCount) &&
            (identical(other.occupancyPercent, occupancyPercent) ||
                other.occupancyPercent == occupancyPercent) &&
            (identical(other.openTime, openTime) ||
                other.openTime == openTime) &&
            (identical(other.closeTime, closeTime) ||
                other.closeTime == closeTime) &&
            (identical(other.capacity, capacity) ||
                other.capacity == capacity) &&
            const DeepCollectionEquality()
                .equals(other._facilities, _facilities) &&
            (identical(other.latitude, latitude) ||
                other.latitude == latitude) &&
            (identical(other.longitude, longitude) ||
                other.longitude == longitude) &&
            (identical(other.description, description) ||
                other.description == description) &&
            (identical(other.hasEntryFee, hasEntryFee) ||
                other.hasEntryFee == hasEntryFee) &&
            (identical(other.isOpen, isOpen) || other.isOpen == isOpen) &&
            (identical(other.sunbedPrice, sunbedPrice) ||
                other.sunbedPrice == sunbedPrice) &&
            (identical(other.phone, phone) || other.phone == phone) &&
            (identical(other.website, website) || other.website == website) &&
            (identical(other.instagram, instagram) ||
                other.instagram == instagram) &&
            (identical(other.hasBar, hasBar) || other.hasBar == hasBar) &&
            (identical(other.hasWaterSports, hasWaterSports) ||
                other.hasWaterSports == hasWaterSports) &&
            (identical(other.isChildFriendly, isChildFriendly) ||
                other.isChildFriendly == isChildFriendly) &&
            (identical(other.hasPool, hasPool) || other.hasPool == hasPool) &&
            (identical(other.hasRestaurant, hasRestaurant) ||
                other.hasRestaurant == hasRestaurant) &&
            (identical(other.hasWifi, hasWifi) || other.hasWifi == hasWifi) &&
            (identical(other.hasParking, hasParking) ||
                other.hasParking == hasParking) &&
            (identical(other.hasSunbeds, hasSunbeds) ||
                other.hasSunbeds == hasSunbeds) &&
            (identical(other.hasShower, hasShower) ||
                other.hasShower == hasShower) &&
            (identical(other.hasDJ, hasDJ) || other.hasDJ == hasDJ));
  }

  @JsonKey(ignore: true)
  @override
  int get hashCode => Object.hashAll([
        runtimeType,
        id,
        name,
        location,
        address,
        imageUrl,
        entryFee,
        rating,
        reviewCount,
        occupancyPercent,
        openTime,
        closeTime,
        capacity,
        const DeepCollectionEquality().hash(_facilities),
        latitude,
        longitude,
        description,
        hasEntryFee,
        isOpen,
        sunbedPrice,
        phone,
        website,
        instagram,
        hasBar,
        hasWaterSports,
        isChildFriendly,
        hasPool,
        hasRestaurant,
        hasWifi,
        hasParking,
        hasSunbeds,
        hasShower,
        hasDJ
      ]);

  @JsonKey(ignore: true)
  @override
  @pragma('vm:prefer-inline')
  _$$BeachDtoImplCopyWith<_$BeachDtoImpl> get copyWith =>
      __$$BeachDtoImplCopyWithImpl<_$BeachDtoImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$BeachDtoImplToJson(
      this,
    );
  }
}

abstract class _BeachDto implements BeachDto {
  const factory _BeachDto(
      {final int? id,
      final String? name,
      final String? location,
      final String? address,
      final String? imageUrl,
      final double? entryFee,
      final double? rating,
      final int? reviewCount,
      final double? occupancyPercent,
      final String? openTime,
      final String? closeTime,
      final int? capacity,
      final List<String>? facilities,
      final double? latitude,
      final double? longitude,
      final String? description,
      final bool? hasEntryFee,
      final bool? isOpen,
      final double? sunbedPrice,
      final String? phone,
      final String? website,
      final String? instagram,
      final bool? hasBar,
      final bool? hasWaterSports,
      final bool? isChildFriendly,
      final bool? hasPool,
      final bool? hasRestaurant,
      final bool? hasWifi,
      final bool? hasParking,
      final bool? hasSunbeds,
      final bool? hasShower,
      final bool? hasDJ}) = _$BeachDtoImpl;

  factory _BeachDto.fromJson(Map<String, dynamic> json) =
      _$BeachDtoImpl.fromJson;

  @override
  int? get id;
  @override
  String? get name;
  @override
  String? get location;
  @override
  String? get address;
  @override
  String? get imageUrl;
  @override
  double? get entryFee;
  @override
  double? get rating;
  @override
  int? get reviewCount;
  @override
  double? get occupancyPercent;
  @override
  String? get openTime;
  @override
  String? get closeTime;
  @override
  int? get capacity;
  @override
  List<String>? get facilities;
  @override
  double? get latitude;
  @override
  double? get longitude;
  @override
  String? get description;
  @override
  bool? get hasEntryFee;
  @override
  bool? get isOpen;
  @override
  double? get sunbedPrice;
  @override
  String? get phone;
  @override
  String? get website;
  @override
  String? get instagram;
  @override
  bool? get hasBar;
  @override
  bool? get hasWaterSports;
  @override
  bool? get isChildFriendly;
  @override
  bool? get hasPool;
  @override
  bool? get hasRestaurant;
  @override
  bool? get hasWifi;
  @override
  bool? get hasParking;
  @override
  bool? get hasSunbeds;
  @override
  bool? get hasShower;
  @override
  bool? get hasDJ;
  @override
  @JsonKey(ignore: true)
  _$$BeachDtoImplCopyWith<_$BeachDtoImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

ReservationDto _$ReservationDtoFromJson(Map<String, dynamic> json) {
  return _ReservationDto.fromJson(json);
}

/// @nodoc
mixin _$ReservationDto {
  int? get id => throw _privateConstructorUsedError;
  String? get status => throw _privateConstructorUsedError;
  String? get paymentStatus => throw _privateConstructorUsedError;
  String? get reservationDate => throw _privateConstructorUsedError;
  String? get confirmationCode => throw _privateConstructorUsedError;
  double? get totalPrice => throw _privateConstructorUsedError;
  String? get paymentUrl => throw _privateConstructorUsedError;
  String? get transactionId => throw _privateConstructorUsedError;
  String? get beachName => throw _privateConstructorUsedError;
  String? get customerName => throw _privateConstructorUsedError;
  String? get reservationTime => throw _privateConstructorUsedError;
  String? get reservationType => throw _privateConstructorUsedError;
  int? get pax => throw _privateConstructorUsedError;

  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;
  @JsonKey(ignore: true)
  $ReservationDtoCopyWith<ReservationDto> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $ReservationDtoCopyWith<$Res> {
  factory $ReservationDtoCopyWith(
          ReservationDto value, $Res Function(ReservationDto) then) =
      _$ReservationDtoCopyWithImpl<$Res, ReservationDto>;
  @useResult
  $Res call(
      {int? id,
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
      int? pax});
}

/// @nodoc
class _$ReservationDtoCopyWithImpl<$Res, $Val extends ReservationDto>
    implements $ReservationDtoCopyWith<$Res> {
  _$ReservationDtoCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? status = freezed,
    Object? paymentStatus = freezed,
    Object? reservationDate = freezed,
    Object? confirmationCode = freezed,
    Object? totalPrice = freezed,
    Object? paymentUrl = freezed,
    Object? transactionId = freezed,
    Object? beachName = freezed,
    Object? customerName = freezed,
    Object? reservationTime = freezed,
    Object? reservationType = freezed,
    Object? pax = freezed,
  }) {
    return _then(_value.copyWith(
      id: freezed == id
          ? _value.id
          : id // ignore: cast_nullable_to_non_nullable
              as int?,
      status: freezed == status
          ? _value.status
          : status // ignore: cast_nullable_to_non_nullable
              as String?,
      paymentStatus: freezed == paymentStatus
          ? _value.paymentStatus
          : paymentStatus // ignore: cast_nullable_to_non_nullable
              as String?,
      reservationDate: freezed == reservationDate
          ? _value.reservationDate
          : reservationDate // ignore: cast_nullable_to_non_nullable
              as String?,
      confirmationCode: freezed == confirmationCode
          ? _value.confirmationCode
          : confirmationCode // ignore: cast_nullable_to_non_nullable
              as String?,
      totalPrice: freezed == totalPrice
          ? _value.totalPrice
          : totalPrice // ignore: cast_nullable_to_non_nullable
              as double?,
      paymentUrl: freezed == paymentUrl
          ? _value.paymentUrl
          : paymentUrl // ignore: cast_nullable_to_non_nullable
              as String?,
      transactionId: freezed == transactionId
          ? _value.transactionId
          : transactionId // ignore: cast_nullable_to_non_nullable
              as String?,
      beachName: freezed == beachName
          ? _value.beachName
          : beachName // ignore: cast_nullable_to_non_nullable
              as String?,
      customerName: freezed == customerName
          ? _value.customerName
          : customerName // ignore: cast_nullable_to_non_nullable
              as String?,
      reservationTime: freezed == reservationTime
          ? _value.reservationTime
          : reservationTime // ignore: cast_nullable_to_non_nullable
              as String?,
      reservationType: freezed == reservationType
          ? _value.reservationType
          : reservationType // ignore: cast_nullable_to_non_nullable
              as String?,
      pax: freezed == pax
          ? _value.pax
          : pax // ignore: cast_nullable_to_non_nullable
              as int?,
    ) as $Val);
  }
}

/// @nodoc
abstract class _$$ReservationDtoImplCopyWith<$Res>
    implements $ReservationDtoCopyWith<$Res> {
  factory _$$ReservationDtoImplCopyWith(_$ReservationDtoImpl value,
          $Res Function(_$ReservationDtoImpl) then) =
      __$$ReservationDtoImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call(
      {int? id,
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
      int? pax});
}

/// @nodoc
class __$$ReservationDtoImplCopyWithImpl<$Res>
    extends _$ReservationDtoCopyWithImpl<$Res, _$ReservationDtoImpl>
    implements _$$ReservationDtoImplCopyWith<$Res> {
  __$$ReservationDtoImplCopyWithImpl(
      _$ReservationDtoImpl _value, $Res Function(_$ReservationDtoImpl) _then)
      : super(_value, _then);

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? status = freezed,
    Object? paymentStatus = freezed,
    Object? reservationDate = freezed,
    Object? confirmationCode = freezed,
    Object? totalPrice = freezed,
    Object? paymentUrl = freezed,
    Object? transactionId = freezed,
    Object? beachName = freezed,
    Object? customerName = freezed,
    Object? reservationTime = freezed,
    Object? reservationType = freezed,
    Object? pax = freezed,
  }) {
    return _then(_$ReservationDtoImpl(
      id: freezed == id
          ? _value.id
          : id // ignore: cast_nullable_to_non_nullable
              as int?,
      status: freezed == status
          ? _value.status
          : status // ignore: cast_nullable_to_non_nullable
              as String?,
      paymentStatus: freezed == paymentStatus
          ? _value.paymentStatus
          : paymentStatus // ignore: cast_nullable_to_non_nullable
              as String?,
      reservationDate: freezed == reservationDate
          ? _value.reservationDate
          : reservationDate // ignore: cast_nullable_to_non_nullable
              as String?,
      confirmationCode: freezed == confirmationCode
          ? _value.confirmationCode
          : confirmationCode // ignore: cast_nullable_to_non_nullable
              as String?,
      totalPrice: freezed == totalPrice
          ? _value.totalPrice
          : totalPrice // ignore: cast_nullable_to_non_nullable
              as double?,
      paymentUrl: freezed == paymentUrl
          ? _value.paymentUrl
          : paymentUrl // ignore: cast_nullable_to_non_nullable
              as String?,
      transactionId: freezed == transactionId
          ? _value.transactionId
          : transactionId // ignore: cast_nullable_to_non_nullable
              as String?,
      beachName: freezed == beachName
          ? _value.beachName
          : beachName // ignore: cast_nullable_to_non_nullable
              as String?,
      customerName: freezed == customerName
          ? _value.customerName
          : customerName // ignore: cast_nullable_to_non_nullable
              as String?,
      reservationTime: freezed == reservationTime
          ? _value.reservationTime
          : reservationTime // ignore: cast_nullable_to_non_nullable
              as String?,
      reservationType: freezed == reservationType
          ? _value.reservationType
          : reservationType // ignore: cast_nullable_to_non_nullable
              as String?,
      pax: freezed == pax
          ? _value.pax
          : pax // ignore: cast_nullable_to_non_nullable
              as int?,
    ));
  }
}

/// @nodoc
@JsonSerializable()
class _$ReservationDtoImpl implements _ReservationDto {
  const _$ReservationDtoImpl(
      {this.id,
      this.status,
      this.paymentStatus,
      this.reservationDate,
      this.confirmationCode,
      this.totalPrice,
      this.paymentUrl,
      this.transactionId,
      this.beachName,
      this.customerName,
      this.reservationTime,
      this.reservationType,
      this.pax});

  factory _$ReservationDtoImpl.fromJson(Map<String, dynamic> json) =>
      _$$ReservationDtoImplFromJson(json);

  @override
  final int? id;
  @override
  final String? status;
  @override
  final String? paymentStatus;
  @override
  final String? reservationDate;
  @override
  final String? confirmationCode;
  @override
  final double? totalPrice;
  @override
  final String? paymentUrl;
  @override
  final String? transactionId;
  @override
  final String? beachName;
  @override
  final String? customerName;
  @override
  final String? reservationTime;
  @override
  final String? reservationType;
  @override
  final int? pax;

  @override
  String toString() {
    return 'ReservationDto(id: $id, status: $status, paymentStatus: $paymentStatus, reservationDate: $reservationDate, confirmationCode: $confirmationCode, totalPrice: $totalPrice, paymentUrl: $paymentUrl, transactionId: $transactionId, beachName: $beachName, customerName: $customerName, reservationTime: $reservationTime, reservationType: $reservationType, pax: $pax)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$ReservationDtoImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.status, status) || other.status == status) &&
            (identical(other.paymentStatus, paymentStatus) ||
                other.paymentStatus == paymentStatus) &&
            (identical(other.reservationDate, reservationDate) ||
                other.reservationDate == reservationDate) &&
            (identical(other.confirmationCode, confirmationCode) ||
                other.confirmationCode == confirmationCode) &&
            (identical(other.totalPrice, totalPrice) ||
                other.totalPrice == totalPrice) &&
            (identical(other.paymentUrl, paymentUrl) ||
                other.paymentUrl == paymentUrl) &&
            (identical(other.transactionId, transactionId) ||
                other.transactionId == transactionId) &&
            (identical(other.beachName, beachName) ||
                other.beachName == beachName) &&
            (identical(other.customerName, customerName) ||
                other.customerName == customerName) &&
            (identical(other.reservationTime, reservationTime) ||
                other.reservationTime == reservationTime) &&
            (identical(other.reservationType, reservationType) ||
                other.reservationType == reservationType) &&
            (identical(other.pax, pax) || other.pax == pax));
  }

  @JsonKey(ignore: true)
  @override
  int get hashCode => Object.hash(
      runtimeType,
      id,
      status,
      paymentStatus,
      reservationDate,
      confirmationCode,
      totalPrice,
      paymentUrl,
      transactionId,
      beachName,
      customerName,
      reservationTime,
      reservationType,
      pax);

  @JsonKey(ignore: true)
  @override
  @pragma('vm:prefer-inline')
  _$$ReservationDtoImplCopyWith<_$ReservationDtoImpl> get copyWith =>
      __$$ReservationDtoImplCopyWithImpl<_$ReservationDtoImpl>(
          this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$ReservationDtoImplToJson(
      this,
    );
  }
}

abstract class _ReservationDto implements ReservationDto {
  const factory _ReservationDto(
      {final int? id,
      final String? status,
      final String? paymentStatus,
      final String? reservationDate,
      final String? confirmationCode,
      final double? totalPrice,
      final String? paymentUrl,
      final String? transactionId,
      final String? beachName,
      final String? customerName,
      final String? reservationTime,
      final String? reservationType,
      final int? pax}) = _$ReservationDtoImpl;

  factory _ReservationDto.fromJson(Map<String, dynamic> json) =
      _$ReservationDtoImpl.fromJson;

  @override
  int? get id;
  @override
  String? get status;
  @override
  String? get paymentStatus;
  @override
  String? get reservationDate;
  @override
  String? get confirmationCode;
  @override
  double? get totalPrice;
  @override
  String? get paymentUrl;
  @override
  String? get transactionId;
  @override
  String? get beachName;
  @override
  String? get customerName;
  @override
  String? get reservationTime;
  @override
  String? get reservationType;
  @override
  int? get pax;
  @override
  @JsonKey(ignore: true)
  _$$ReservationDtoImplCopyWith<_$ReservationDtoImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

EventDto _$EventDtoFromJson(Map<String, dynamic> json) {
  return _EventDto.fromJson(json);
}

/// @nodoc
mixin _$EventDto {
  dynamic get id => throw _privateConstructorUsedError;
  String? get startDate => throw _privateConstructorUsedError;
  String? get date => throw _privateConstructorUsedError;
  String? get title => throw _privateConstructorUsedError;
  String? get imageUrl => throw _privateConstructorUsedError;
  bool? get isActive => throw _privateConstructorUsedError;
  String? get description => throw _privateConstructorUsedError;
  String? get beachName => throw _privateConstructorUsedError;
  int? get beachId => throw _privateConstructorUsedError;

  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;
  @JsonKey(ignore: true)
  $EventDtoCopyWith<EventDto> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $EventDtoCopyWith<$Res> {
  factory $EventDtoCopyWith(EventDto value, $Res Function(EventDto) then) =
      _$EventDtoCopyWithImpl<$Res, EventDto>;
  @useResult
  $Res call(
      {dynamic id,
      String? startDate,
      String? date,
      String? title,
      String? imageUrl,
      bool? isActive,
      String? description,
      String? beachName,
      int? beachId});
}

/// @nodoc
class _$EventDtoCopyWithImpl<$Res, $Val extends EventDto>
    implements $EventDtoCopyWith<$Res> {
  _$EventDtoCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? startDate = freezed,
    Object? date = freezed,
    Object? title = freezed,
    Object? imageUrl = freezed,
    Object? isActive = freezed,
    Object? description = freezed,
    Object? beachName = freezed,
    Object? beachId = freezed,
  }) {
    return _then(_value.copyWith(
      id: freezed == id
          ? _value.id
          : id // ignore: cast_nullable_to_non_nullable
              as dynamic,
      startDate: freezed == startDate
          ? _value.startDate
          : startDate // ignore: cast_nullable_to_non_nullable
              as String?,
      date: freezed == date
          ? _value.date
          : date // ignore: cast_nullable_to_non_nullable
              as String?,
      title: freezed == title
          ? _value.title
          : title // ignore: cast_nullable_to_non_nullable
              as String?,
      imageUrl: freezed == imageUrl
          ? _value.imageUrl
          : imageUrl // ignore: cast_nullable_to_non_nullable
              as String?,
      isActive: freezed == isActive
          ? _value.isActive
          : isActive // ignore: cast_nullable_to_non_nullable
              as bool?,
      description: freezed == description
          ? _value.description
          : description // ignore: cast_nullable_to_non_nullable
              as String?,
      beachName: freezed == beachName
          ? _value.beachName
          : beachName // ignore: cast_nullable_to_non_nullable
              as String?,
      beachId: freezed == beachId
          ? _value.beachId
          : beachId // ignore: cast_nullable_to_non_nullable
              as int?,
    ) as $Val);
  }
}

/// @nodoc
abstract class _$$EventDtoImplCopyWith<$Res>
    implements $EventDtoCopyWith<$Res> {
  factory _$$EventDtoImplCopyWith(
          _$EventDtoImpl value, $Res Function(_$EventDtoImpl) then) =
      __$$EventDtoImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call(
      {dynamic id,
      String? startDate,
      String? date,
      String? title,
      String? imageUrl,
      bool? isActive,
      String? description,
      String? beachName,
      int? beachId});
}

/// @nodoc
class __$$EventDtoImplCopyWithImpl<$Res>
    extends _$EventDtoCopyWithImpl<$Res, _$EventDtoImpl>
    implements _$$EventDtoImplCopyWith<$Res> {
  __$$EventDtoImplCopyWithImpl(
      _$EventDtoImpl _value, $Res Function(_$EventDtoImpl) _then)
      : super(_value, _then);

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? startDate = freezed,
    Object? date = freezed,
    Object? title = freezed,
    Object? imageUrl = freezed,
    Object? isActive = freezed,
    Object? description = freezed,
    Object? beachName = freezed,
    Object? beachId = freezed,
  }) {
    return _then(_$EventDtoImpl(
      id: freezed == id
          ? _value.id
          : id // ignore: cast_nullable_to_non_nullable
              as dynamic,
      startDate: freezed == startDate
          ? _value.startDate
          : startDate // ignore: cast_nullable_to_non_nullable
              as String?,
      date: freezed == date
          ? _value.date
          : date // ignore: cast_nullable_to_non_nullable
              as String?,
      title: freezed == title
          ? _value.title
          : title // ignore: cast_nullable_to_non_nullable
              as String?,
      imageUrl: freezed == imageUrl
          ? _value.imageUrl
          : imageUrl // ignore: cast_nullable_to_non_nullable
              as String?,
      isActive: freezed == isActive
          ? _value.isActive
          : isActive // ignore: cast_nullable_to_non_nullable
              as bool?,
      description: freezed == description
          ? _value.description
          : description // ignore: cast_nullable_to_non_nullable
              as String?,
      beachName: freezed == beachName
          ? _value.beachName
          : beachName // ignore: cast_nullable_to_non_nullable
              as String?,
      beachId: freezed == beachId
          ? _value.beachId
          : beachId // ignore: cast_nullable_to_non_nullable
              as int?,
    ));
  }
}

/// @nodoc
@JsonSerializable()
class _$EventDtoImpl implements _EventDto {
  const _$EventDtoImpl(
      {this.id,
      this.startDate,
      this.date,
      this.title,
      this.imageUrl,
      this.isActive,
      this.description,
      this.beachName,
      this.beachId});

  factory _$EventDtoImpl.fromJson(Map<String, dynamic> json) =>
      _$$EventDtoImplFromJson(json);

  @override
  final dynamic id;
  @override
  final String? startDate;
  @override
  final String? date;
  @override
  final String? title;
  @override
  final String? imageUrl;
  @override
  final bool? isActive;
  @override
  final String? description;
  @override
  final String? beachName;
  @override
  final int? beachId;

  @override
  String toString() {
    return 'EventDto(id: $id, startDate: $startDate, date: $date, title: $title, imageUrl: $imageUrl, isActive: $isActive, description: $description, beachName: $beachName, beachId: $beachId)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$EventDtoImpl &&
            const DeepCollectionEquality().equals(other.id, id) &&
            (identical(other.startDate, startDate) ||
                other.startDate == startDate) &&
            (identical(other.date, date) || other.date == date) &&
            (identical(other.title, title) || other.title == title) &&
            (identical(other.imageUrl, imageUrl) ||
                other.imageUrl == imageUrl) &&
            (identical(other.isActive, isActive) ||
                other.isActive == isActive) &&
            (identical(other.description, description) ||
                other.description == description) &&
            (identical(other.beachName, beachName) ||
                other.beachName == beachName) &&
            (identical(other.beachId, beachId) || other.beachId == beachId));
  }

  @JsonKey(ignore: true)
  @override
  int get hashCode => Object.hash(
      runtimeType,
      const DeepCollectionEquality().hash(id),
      startDate,
      date,
      title,
      imageUrl,
      isActive,
      description,
      beachName,
      beachId);

  @JsonKey(ignore: true)
  @override
  @pragma('vm:prefer-inline')
  _$$EventDtoImplCopyWith<_$EventDtoImpl> get copyWith =>
      __$$EventDtoImplCopyWithImpl<_$EventDtoImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$EventDtoImplToJson(
      this,
    );
  }
}

abstract class _EventDto implements EventDto {
  const factory _EventDto(
      {final dynamic id,
      final String? startDate,
      final String? date,
      final String? title,
      final String? imageUrl,
      final bool? isActive,
      final String? description,
      final String? beachName,
      final int? beachId}) = _$EventDtoImpl;

  factory _EventDto.fromJson(Map<String, dynamic> json) =
      _$EventDtoImpl.fromJson;

  @override
  dynamic get id;
  @override
  String? get startDate;
  @override
  String? get date;
  @override
  String? get title;
  @override
  String? get imageUrl;
  @override
  bool? get isActive;
  @override
  String? get description;
  @override
  String? get beachName;
  @override
  int? get beachId;
  @override
  @JsonKey(ignore: true)
  _$$EventDtoImplCopyWith<_$EventDtoImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

BeachReviewDto _$BeachReviewDtoFromJson(Map<String, dynamic> json) {
  return _BeachReviewDto.fromJson(json);
}

/// @nodoc
mixin _$BeachReviewDto {
  String? get userName => throw _privateConstructorUsedError;
  String? get createdAt => throw _privateConstructorUsedError;
  double? get rating => throw _privateConstructorUsedError;
  String? get comment => throw _privateConstructorUsedError;

  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;
  @JsonKey(ignore: true)
  $BeachReviewDtoCopyWith<BeachReviewDto> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $BeachReviewDtoCopyWith<$Res> {
  factory $BeachReviewDtoCopyWith(
          BeachReviewDto value, $Res Function(BeachReviewDto) then) =
      _$BeachReviewDtoCopyWithImpl<$Res, BeachReviewDto>;
  @useResult
  $Res call(
      {String? userName, String? createdAt, double? rating, String? comment});
}

/// @nodoc
class _$BeachReviewDtoCopyWithImpl<$Res, $Val extends BeachReviewDto>
    implements $BeachReviewDtoCopyWith<$Res> {
  _$BeachReviewDtoCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? userName = freezed,
    Object? createdAt = freezed,
    Object? rating = freezed,
    Object? comment = freezed,
  }) {
    return _then(_value.copyWith(
      userName: freezed == userName
          ? _value.userName
          : userName // ignore: cast_nullable_to_non_nullable
              as String?,
      createdAt: freezed == createdAt
          ? _value.createdAt
          : createdAt // ignore: cast_nullable_to_non_nullable
              as String?,
      rating: freezed == rating
          ? _value.rating
          : rating // ignore: cast_nullable_to_non_nullable
              as double?,
      comment: freezed == comment
          ? _value.comment
          : comment // ignore: cast_nullable_to_non_nullable
              as String?,
    ) as $Val);
  }
}

/// @nodoc
abstract class _$$BeachReviewDtoImplCopyWith<$Res>
    implements $BeachReviewDtoCopyWith<$Res> {
  factory _$$BeachReviewDtoImplCopyWith(_$BeachReviewDtoImpl value,
          $Res Function(_$BeachReviewDtoImpl) then) =
      __$$BeachReviewDtoImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call(
      {String? userName, String? createdAt, double? rating, String? comment});
}

/// @nodoc
class __$$BeachReviewDtoImplCopyWithImpl<$Res>
    extends _$BeachReviewDtoCopyWithImpl<$Res, _$BeachReviewDtoImpl>
    implements _$$BeachReviewDtoImplCopyWith<$Res> {
  __$$BeachReviewDtoImplCopyWithImpl(
      _$BeachReviewDtoImpl _value, $Res Function(_$BeachReviewDtoImpl) _then)
      : super(_value, _then);

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? userName = freezed,
    Object? createdAt = freezed,
    Object? rating = freezed,
    Object? comment = freezed,
  }) {
    return _then(_$BeachReviewDtoImpl(
      userName: freezed == userName
          ? _value.userName
          : userName // ignore: cast_nullable_to_non_nullable
              as String?,
      createdAt: freezed == createdAt
          ? _value.createdAt
          : createdAt // ignore: cast_nullable_to_non_nullable
              as String?,
      rating: freezed == rating
          ? _value.rating
          : rating // ignore: cast_nullable_to_non_nullable
              as double?,
      comment: freezed == comment
          ? _value.comment
          : comment // ignore: cast_nullable_to_non_nullable
              as String?,
    ));
  }
}

/// @nodoc
@JsonSerializable()
class _$BeachReviewDtoImpl implements _BeachReviewDto {
  const _$BeachReviewDtoImpl(
      {this.userName, this.createdAt, this.rating, this.comment});

  factory _$BeachReviewDtoImpl.fromJson(Map<String, dynamic> json) =>
      _$$BeachReviewDtoImplFromJson(json);

  @override
  final String? userName;
  @override
  final String? createdAt;
  @override
  final double? rating;
  @override
  final String? comment;

  @override
  String toString() {
    return 'BeachReviewDto(userName: $userName, createdAt: $createdAt, rating: $rating, comment: $comment)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$BeachReviewDtoImpl &&
            (identical(other.userName, userName) ||
                other.userName == userName) &&
            (identical(other.createdAt, createdAt) ||
                other.createdAt == createdAt) &&
            (identical(other.rating, rating) || other.rating == rating) &&
            (identical(other.comment, comment) || other.comment == comment));
  }

  @JsonKey(ignore: true)
  @override
  int get hashCode =>
      Object.hash(runtimeType, userName, createdAt, rating, comment);

  @JsonKey(ignore: true)
  @override
  @pragma('vm:prefer-inline')
  _$$BeachReviewDtoImplCopyWith<_$BeachReviewDtoImpl> get copyWith =>
      __$$BeachReviewDtoImplCopyWithImpl<_$BeachReviewDtoImpl>(
          this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$BeachReviewDtoImplToJson(
      this,
    );
  }
}

abstract class _BeachReviewDto implements BeachReviewDto {
  const factory _BeachReviewDto(
      {final String? userName,
      final String? createdAt,
      final double? rating,
      final String? comment}) = _$BeachReviewDtoImpl;

  factory _BeachReviewDto.fromJson(Map<String, dynamic> json) =
      _$BeachReviewDtoImpl.fromJson;

  @override
  String? get userName;
  @override
  String? get createdAt;
  @override
  double? get rating;
  @override
  String? get comment;
  @override
  @JsonKey(ignore: true)
  _$$BeachReviewDtoImplCopyWith<_$BeachReviewDtoImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

FavoriteDto _$FavoriteDtoFromJson(Map<String, dynamic> json) {
  return _FavoriteDto.fromJson(json);
}

/// @nodoc
mixin _$FavoriteDto {
  int? get id => throw _privateConstructorUsedError;
  int? get beachId => throw _privateConstructorUsedError;
  BeachDto? get beach => throw _privateConstructorUsedError;

  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;
  @JsonKey(ignore: true)
  $FavoriteDtoCopyWith<FavoriteDto> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $FavoriteDtoCopyWith<$Res> {
  factory $FavoriteDtoCopyWith(
          FavoriteDto value, $Res Function(FavoriteDto) then) =
      _$FavoriteDtoCopyWithImpl<$Res, FavoriteDto>;
  @useResult
  $Res call({int? id, int? beachId, BeachDto? beach});

  $BeachDtoCopyWith<$Res>? get beach;
}

/// @nodoc
class _$FavoriteDtoCopyWithImpl<$Res, $Val extends FavoriteDto>
    implements $FavoriteDtoCopyWith<$Res> {
  _$FavoriteDtoCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? beachId = freezed,
    Object? beach = freezed,
  }) {
    return _then(_value.copyWith(
      id: freezed == id
          ? _value.id
          : id // ignore: cast_nullable_to_non_nullable
              as int?,
      beachId: freezed == beachId
          ? _value.beachId
          : beachId // ignore: cast_nullable_to_non_nullable
              as int?,
      beach: freezed == beach
          ? _value.beach
          : beach // ignore: cast_nullable_to_non_nullable
              as BeachDto?,
    ) as $Val);
  }

  @override
  @pragma('vm:prefer-inline')
  $BeachDtoCopyWith<$Res>? get beach {
    if (_value.beach == null) {
      return null;
    }

    return $BeachDtoCopyWith<$Res>(_value.beach!, (value) {
      return _then(_value.copyWith(beach: value) as $Val);
    });
  }
}

/// @nodoc
abstract class _$$FavoriteDtoImplCopyWith<$Res>
    implements $FavoriteDtoCopyWith<$Res> {
  factory _$$FavoriteDtoImplCopyWith(
          _$FavoriteDtoImpl value, $Res Function(_$FavoriteDtoImpl) then) =
      __$$FavoriteDtoImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call({int? id, int? beachId, BeachDto? beach});

  @override
  $BeachDtoCopyWith<$Res>? get beach;
}

/// @nodoc
class __$$FavoriteDtoImplCopyWithImpl<$Res>
    extends _$FavoriteDtoCopyWithImpl<$Res, _$FavoriteDtoImpl>
    implements _$$FavoriteDtoImplCopyWith<$Res> {
  __$$FavoriteDtoImplCopyWithImpl(
      _$FavoriteDtoImpl _value, $Res Function(_$FavoriteDtoImpl) _then)
      : super(_value, _then);

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? id = freezed,
    Object? beachId = freezed,
    Object? beach = freezed,
  }) {
    return _then(_$FavoriteDtoImpl(
      id: freezed == id
          ? _value.id
          : id // ignore: cast_nullable_to_non_nullable
              as int?,
      beachId: freezed == beachId
          ? _value.beachId
          : beachId // ignore: cast_nullable_to_non_nullable
              as int?,
      beach: freezed == beach
          ? _value.beach
          : beach // ignore: cast_nullable_to_non_nullable
              as BeachDto?,
    ));
  }
}

/// @nodoc
@JsonSerializable()
class _$FavoriteDtoImpl implements _FavoriteDto {
  const _$FavoriteDtoImpl({this.id, this.beachId, this.beach});

  factory _$FavoriteDtoImpl.fromJson(Map<String, dynamic> json) =>
      _$$FavoriteDtoImplFromJson(json);

  @override
  final int? id;
  @override
  final int? beachId;
  @override
  final BeachDto? beach;

  @override
  String toString() {
    return 'FavoriteDto(id: $id, beachId: $beachId, beach: $beach)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$FavoriteDtoImpl &&
            (identical(other.id, id) || other.id == id) &&
            (identical(other.beachId, beachId) || other.beachId == beachId) &&
            (identical(other.beach, beach) || other.beach == beach));
  }

  @JsonKey(ignore: true)
  @override
  int get hashCode => Object.hash(runtimeType, id, beachId, beach);

  @JsonKey(ignore: true)
  @override
  @pragma('vm:prefer-inline')
  _$$FavoriteDtoImplCopyWith<_$FavoriteDtoImpl> get copyWith =>
      __$$FavoriteDtoImplCopyWithImpl<_$FavoriteDtoImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$FavoriteDtoImplToJson(
      this,
    );
  }
}

abstract class _FavoriteDto implements FavoriteDto {
  const factory _FavoriteDto(
      {final int? id,
      final int? beachId,
      final BeachDto? beach}) = _$FavoriteDtoImpl;

  factory _FavoriteDto.fromJson(Map<String, dynamic> json) =
      _$FavoriteDtoImpl.fromJson;

  @override
  int? get id;
  @override
  int? get beachId;
  @override
  BeachDto? get beach;
  @override
  @JsonKey(ignore: true)
  _$$FavoriteDtoImplCopyWith<_$FavoriteDtoImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

AuthResponse _$AuthResponseFromJson(Map<String, dynamic> json) {
  return _AuthResponse.fromJson(json);
}

/// @nodoc
mixin _$AuthResponse {
  String? get accessToken => throw _privateConstructorUsedError;
  String? get token => throw _privateConstructorUsedError;
  String? get refreshToken => throw _privateConstructorUsedError;
  AppUser? get user => throw _privateConstructorUsedError;
  String? get role => throw _privateConstructorUsedError;
  String? get accountType => throw _privateConstructorUsedError;

  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;
  @JsonKey(ignore: true)
  $AuthResponseCopyWith<AuthResponse> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $AuthResponseCopyWith<$Res> {
  factory $AuthResponseCopyWith(
          AuthResponse value, $Res Function(AuthResponse) then) =
      _$AuthResponseCopyWithImpl<$Res, AuthResponse>;
  @useResult
  $Res call(
      {String? accessToken,
      String? token,
      String? refreshToken,
      AppUser? user,
      String? role,
      String? accountType});

  $AppUserCopyWith<$Res>? get user;
}

/// @nodoc
class _$AuthResponseCopyWithImpl<$Res, $Val extends AuthResponse>
    implements $AuthResponseCopyWith<$Res> {
  _$AuthResponseCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? accessToken = freezed,
    Object? token = freezed,
    Object? refreshToken = freezed,
    Object? user = freezed,
    Object? role = freezed,
    Object? accountType = freezed,
  }) {
    return _then(_value.copyWith(
      accessToken: freezed == accessToken
          ? _value.accessToken
          : accessToken // ignore: cast_nullable_to_non_nullable
              as String?,
      token: freezed == token
          ? _value.token
          : token // ignore: cast_nullable_to_non_nullable
              as String?,
      refreshToken: freezed == refreshToken
          ? _value.refreshToken
          : refreshToken // ignore: cast_nullable_to_non_nullable
              as String?,
      user: freezed == user
          ? _value.user
          : user // ignore: cast_nullable_to_non_nullable
              as AppUser?,
      role: freezed == role
          ? _value.role
          : role // ignore: cast_nullable_to_non_nullable
              as String?,
      accountType: freezed == accountType
          ? _value.accountType
          : accountType // ignore: cast_nullable_to_non_nullable
              as String?,
    ) as $Val);
  }

  @override
  @pragma('vm:prefer-inline')
  $AppUserCopyWith<$Res>? get user {
    if (_value.user == null) {
      return null;
    }

    return $AppUserCopyWith<$Res>(_value.user!, (value) {
      return _then(_value.copyWith(user: value) as $Val);
    });
  }
}

/// @nodoc
abstract class _$$AuthResponseImplCopyWith<$Res>
    implements $AuthResponseCopyWith<$Res> {
  factory _$$AuthResponseImplCopyWith(
          _$AuthResponseImpl value, $Res Function(_$AuthResponseImpl) then) =
      __$$AuthResponseImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call(
      {String? accessToken,
      String? token,
      String? refreshToken,
      AppUser? user,
      String? role,
      String? accountType});

  @override
  $AppUserCopyWith<$Res>? get user;
}

/// @nodoc
class __$$AuthResponseImplCopyWithImpl<$Res>
    extends _$AuthResponseCopyWithImpl<$Res, _$AuthResponseImpl>
    implements _$$AuthResponseImplCopyWith<$Res> {
  __$$AuthResponseImplCopyWithImpl(
      _$AuthResponseImpl _value, $Res Function(_$AuthResponseImpl) _then)
      : super(_value, _then);

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? accessToken = freezed,
    Object? token = freezed,
    Object? refreshToken = freezed,
    Object? user = freezed,
    Object? role = freezed,
    Object? accountType = freezed,
  }) {
    return _then(_$AuthResponseImpl(
      accessToken: freezed == accessToken
          ? _value.accessToken
          : accessToken // ignore: cast_nullable_to_non_nullable
              as String?,
      token: freezed == token
          ? _value.token
          : token // ignore: cast_nullable_to_non_nullable
              as String?,
      refreshToken: freezed == refreshToken
          ? _value.refreshToken
          : refreshToken // ignore: cast_nullable_to_non_nullable
              as String?,
      user: freezed == user
          ? _value.user
          : user // ignore: cast_nullable_to_non_nullable
              as AppUser?,
      role: freezed == role
          ? _value.role
          : role // ignore: cast_nullable_to_non_nullable
              as String?,
      accountType: freezed == accountType
          ? _value.accountType
          : accountType // ignore: cast_nullable_to_non_nullable
              as String?,
    ));
  }
}

/// @nodoc
@JsonSerializable()
class _$AuthResponseImpl implements _AuthResponse {
  const _$AuthResponseImpl(
      {this.accessToken,
      this.token,
      this.refreshToken,
      this.user,
      this.role,
      this.accountType});

  factory _$AuthResponseImpl.fromJson(Map<String, dynamic> json) =>
      _$$AuthResponseImplFromJson(json);

  @override
  final String? accessToken;
  @override
  final String? token;
  @override
  final String? refreshToken;
  @override
  final AppUser? user;
  @override
  final String? role;
  @override
  final String? accountType;

  @override
  String toString() {
    return 'AuthResponse(accessToken: $accessToken, token: $token, refreshToken: $refreshToken, user: $user, role: $role, accountType: $accountType)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$AuthResponseImpl &&
            (identical(other.accessToken, accessToken) ||
                other.accessToken == accessToken) &&
            (identical(other.token, token) || other.token == token) &&
            (identical(other.refreshToken, refreshToken) ||
                other.refreshToken == refreshToken) &&
            (identical(other.user, user) || other.user == user) &&
            (identical(other.role, role) || other.role == role) &&
            (identical(other.accountType, accountType) ||
                other.accountType == accountType));
  }

  @JsonKey(ignore: true)
  @override
  int get hashCode => Object.hash(
      runtimeType, accessToken, token, refreshToken, user, role, accountType);

  @JsonKey(ignore: true)
  @override
  @pragma('vm:prefer-inline')
  _$$AuthResponseImplCopyWith<_$AuthResponseImpl> get copyWith =>
      __$$AuthResponseImplCopyWithImpl<_$AuthResponseImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$AuthResponseImplToJson(
      this,
    );
  }
}

abstract class _AuthResponse implements AuthResponse {
  const factory _AuthResponse(
      {final String? accessToken,
      final String? token,
      final String? refreshToken,
      final AppUser? user,
      final String? role,
      final String? accountType}) = _$AuthResponseImpl;

  factory _AuthResponse.fromJson(Map<String, dynamic> json) =
      _$AuthResponseImpl.fromJson;

  @override
  String? get accessToken;
  @override
  String? get token;
  @override
  String? get refreshToken;
  @override
  AppUser? get user;
  @override
  String? get role;
  @override
  String? get accountType;
  @override
  @JsonKey(ignore: true)
  _$$AuthResponseImplCopyWith<_$AuthResponseImpl> get copyWith =>
      throw _privateConstructorUsedError;
}

BeachFilter _$BeachFilterFromJson(Map<String, dynamic> json) {
  return _BeachFilter.fromJson(json);
}

/// @nodoc
mixin _$BeachFilter {
  double? get minRating => throw _privateConstructorUsedError;
  bool? get hasBar => throw _privateConstructorUsedError;
  bool? get hasWaterSports => throw _privateConstructorUsedError;
  bool? get isChildFriendly => throw _privateConstructorUsedError;
  bool? get hasPool => throw _privateConstructorUsedError;
  bool? get freeEntry => throw _privateConstructorUsedError;
  String? get sortBy => throw _privateConstructorUsedError;

  Map<String, dynamic> toJson() => throw _privateConstructorUsedError;
  @JsonKey(ignore: true)
  $BeachFilterCopyWith<BeachFilter> get copyWith =>
      throw _privateConstructorUsedError;
}

/// @nodoc
abstract class $BeachFilterCopyWith<$Res> {
  factory $BeachFilterCopyWith(
          BeachFilter value, $Res Function(BeachFilter) then) =
      _$BeachFilterCopyWithImpl<$Res, BeachFilter>;
  @useResult
  $Res call(
      {double? minRating,
      bool? hasBar,
      bool? hasWaterSports,
      bool? isChildFriendly,
      bool? hasPool,
      bool? freeEntry,
      String? sortBy});
}

/// @nodoc
class _$BeachFilterCopyWithImpl<$Res, $Val extends BeachFilter>
    implements $BeachFilterCopyWith<$Res> {
  _$BeachFilterCopyWithImpl(this._value, this._then);

  // ignore: unused_field
  final $Val _value;
  // ignore: unused_field
  final $Res Function($Val) _then;

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? minRating = freezed,
    Object? hasBar = freezed,
    Object? hasWaterSports = freezed,
    Object? isChildFriendly = freezed,
    Object? hasPool = freezed,
    Object? freeEntry = freezed,
    Object? sortBy = freezed,
  }) {
    return _then(_value.copyWith(
      minRating: freezed == minRating
          ? _value.minRating
          : minRating // ignore: cast_nullable_to_non_nullable
              as double?,
      hasBar: freezed == hasBar
          ? _value.hasBar
          : hasBar // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasWaterSports: freezed == hasWaterSports
          ? _value.hasWaterSports
          : hasWaterSports // ignore: cast_nullable_to_non_nullable
              as bool?,
      isChildFriendly: freezed == isChildFriendly
          ? _value.isChildFriendly
          : isChildFriendly // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasPool: freezed == hasPool
          ? _value.hasPool
          : hasPool // ignore: cast_nullable_to_non_nullable
              as bool?,
      freeEntry: freezed == freeEntry
          ? _value.freeEntry
          : freeEntry // ignore: cast_nullable_to_non_nullable
              as bool?,
      sortBy: freezed == sortBy
          ? _value.sortBy
          : sortBy // ignore: cast_nullable_to_non_nullable
              as String?,
    ) as $Val);
  }
}

/// @nodoc
abstract class _$$BeachFilterImplCopyWith<$Res>
    implements $BeachFilterCopyWith<$Res> {
  factory _$$BeachFilterImplCopyWith(
          _$BeachFilterImpl value, $Res Function(_$BeachFilterImpl) then) =
      __$$BeachFilterImplCopyWithImpl<$Res>;
  @override
  @useResult
  $Res call(
      {double? minRating,
      bool? hasBar,
      bool? hasWaterSports,
      bool? isChildFriendly,
      bool? hasPool,
      bool? freeEntry,
      String? sortBy});
}

/// @nodoc
class __$$BeachFilterImplCopyWithImpl<$Res>
    extends _$BeachFilterCopyWithImpl<$Res, _$BeachFilterImpl>
    implements _$$BeachFilterImplCopyWith<$Res> {
  __$$BeachFilterImplCopyWithImpl(
      _$BeachFilterImpl _value, $Res Function(_$BeachFilterImpl) _then)
      : super(_value, _then);

  @pragma('vm:prefer-inline')
  @override
  $Res call({
    Object? minRating = freezed,
    Object? hasBar = freezed,
    Object? hasWaterSports = freezed,
    Object? isChildFriendly = freezed,
    Object? hasPool = freezed,
    Object? freeEntry = freezed,
    Object? sortBy = freezed,
  }) {
    return _then(_$BeachFilterImpl(
      minRating: freezed == minRating
          ? _value.minRating
          : minRating // ignore: cast_nullable_to_non_nullable
              as double?,
      hasBar: freezed == hasBar
          ? _value.hasBar
          : hasBar // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasWaterSports: freezed == hasWaterSports
          ? _value.hasWaterSports
          : hasWaterSports // ignore: cast_nullable_to_non_nullable
              as bool?,
      isChildFriendly: freezed == isChildFriendly
          ? _value.isChildFriendly
          : isChildFriendly // ignore: cast_nullable_to_non_nullable
              as bool?,
      hasPool: freezed == hasPool
          ? _value.hasPool
          : hasPool // ignore: cast_nullable_to_non_nullable
              as bool?,
      freeEntry: freezed == freeEntry
          ? _value.freeEntry
          : freeEntry // ignore: cast_nullable_to_non_nullable
              as bool?,
      sortBy: freezed == sortBy
          ? _value.sortBy
          : sortBy // ignore: cast_nullable_to_non_nullable
              as String?,
    ));
  }
}

/// @nodoc
@JsonSerializable()
class _$BeachFilterImpl implements _BeachFilter {
  const _$BeachFilterImpl(
      {this.minRating,
      this.hasBar,
      this.hasWaterSports,
      this.isChildFriendly,
      this.hasPool,
      this.freeEntry,
      this.sortBy});

  factory _$BeachFilterImpl.fromJson(Map<String, dynamic> json) =>
      _$$BeachFilterImplFromJson(json);

  @override
  final double? minRating;
  @override
  final bool? hasBar;
  @override
  final bool? hasWaterSports;
  @override
  final bool? isChildFriendly;
  @override
  final bool? hasPool;
  @override
  final bool? freeEntry;
  @override
  final String? sortBy;

  @override
  String toString() {
    return 'BeachFilter(minRating: $minRating, hasBar: $hasBar, hasWaterSports: $hasWaterSports, isChildFriendly: $isChildFriendly, hasPool: $hasPool, freeEntry: $freeEntry, sortBy: $sortBy)';
  }

  @override
  bool operator ==(Object other) {
    return identical(this, other) ||
        (other.runtimeType == runtimeType &&
            other is _$BeachFilterImpl &&
            (identical(other.minRating, minRating) ||
                other.minRating == minRating) &&
            (identical(other.hasBar, hasBar) || other.hasBar == hasBar) &&
            (identical(other.hasWaterSports, hasWaterSports) ||
                other.hasWaterSports == hasWaterSports) &&
            (identical(other.isChildFriendly, isChildFriendly) ||
                other.isChildFriendly == isChildFriendly) &&
            (identical(other.hasPool, hasPool) || other.hasPool == hasPool) &&
            (identical(other.freeEntry, freeEntry) ||
                other.freeEntry == freeEntry) &&
            (identical(other.sortBy, sortBy) || other.sortBy == sortBy));
  }

  @JsonKey(ignore: true)
  @override
  int get hashCode => Object.hash(runtimeType, minRating, hasBar,
      hasWaterSports, isChildFriendly, hasPool, freeEntry, sortBy);

  @JsonKey(ignore: true)
  @override
  @pragma('vm:prefer-inline')
  _$$BeachFilterImplCopyWith<_$BeachFilterImpl> get copyWith =>
      __$$BeachFilterImplCopyWithImpl<_$BeachFilterImpl>(this, _$identity);

  @override
  Map<String, dynamic> toJson() {
    return _$$BeachFilterImplToJson(
      this,
    );
  }
}

abstract class _BeachFilter implements BeachFilter {
  const factory _BeachFilter(
      {final double? minRating,
      final bool? hasBar,
      final bool? hasWaterSports,
      final bool? isChildFriendly,
      final bool? hasPool,
      final bool? freeEntry,
      final String? sortBy}) = _$BeachFilterImpl;

  factory _BeachFilter.fromJson(Map<String, dynamic> json) =
      _$BeachFilterImpl.fromJson;

  @override
  double? get minRating;
  @override
  bool? get hasBar;
  @override
  bool? get hasWaterSports;
  @override
  bool? get isChildFriendly;
  @override
  bool? get hasPool;
  @override
  bool? get freeEntry;
  @override
  String? get sortBy;
  @override
  @JsonKey(ignore: true)
  _$$BeachFilterImplCopyWith<_$BeachFilterImpl> get copyWith =>
      throw _privateConstructorUsedError;
}
