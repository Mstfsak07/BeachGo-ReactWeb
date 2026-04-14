import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/models/models.dart';
import 'package:beachgo/core/network/api_response.dart';
import 'package:beachgo/core/network/dio_client.dart';

final beachRepositoryProvider = Provider<BeachRepository>((ref) {
  return BeachRepository(ref.watch(dioProvider));
});

class BeachRepository {
  BeachRepository(this._dio);

  final Dio _dio;

  static final List<BeachDto> _mockBeaches = [
    const BeachDto(
      id: 1,
      name: 'Konyaalti Blue Coast',
      location: 'Konyaalti',
      address: 'Konyaalti Sahili, Antalya',
      imageUrl:
          'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1200&q=80',
      entryFee: 0,
      rating: 4.7,
      reviewCount: 218,
      occupancyPercent: 54,
      openTime: '08:00',
      closeTime: '22:00',
      capacity: 450,
      facilities: ['Bar', 'Wifi', 'Dus', 'Sezlong'],
      latitude: 36.8599,
      longitude: 30.6360,
      description:
          'Genis yuruyus alani, berrak deniz ve sehir merkezine yakin konumuyla hizli bir sahil kacamagi icin ideal.',
      hasEntryFee: false,
      isOpen: true,
      sunbedPrice: 250,
      phone: '+90 242 000 00 01',
      website: 'konyaaltiblue.example.com',
      instagram: '@konyaaltiblue',
      hasBar: true,
      hasWaterSports: true,
      isChildFriendly: true,
      hasPool: false,
      hasRestaurant: true,
      hasWifi: true,
      hasParking: true,
      hasSunbeds: true,
      hasShower: true,
      hasDJ: false,
    ),
    const BeachDto(
      id: 2,
      name: 'Lara Pearl Beach',
      location: 'Lara',
      address: 'Lara Kiyisi, Antalya',
      imageUrl:
          'https://images.unsplash.com/photo-1500375592092-40eb2168fd21?auto=format&fit=crop&w=1200&q=80',
      entryFee: 350,
      rating: 4.8,
      reviewCount: 184,
      occupancyPercent: 72,
      openTime: '09:00',
      closeTime: '23:30',
      capacity: 280,
      facilities: ['Havuz', 'Restoran', 'DJ', 'Vale'],
      latitude: 36.8485,
      longitude: 30.7845,
      description:
          'Premium servis, aksam DJ setleri ve restoran deneyimi ile daha canli bir beach club atmosferi sunar.',
      hasEntryFee: true,
      isOpen: true,
      sunbedPrice: 400,
      phone: '+90 242 000 00 02',
      website: 'larapearl.example.com',
      instagram: '@larapearl',
      hasBar: true,
      hasWaterSports: false,
      isChildFriendly: false,
      hasPool: true,
      hasRestaurant: true,
      hasWifi: true,
      hasParking: true,
      hasSunbeds: true,
      hasShower: true,
      hasDJ: true,
    ),
    const BeachDto(
      id: 3,
      name: 'Phaselis Family Cove',
      location: 'Kemer',
      address: 'Tekirova Yolu, Kemer',
      imageUrl:
          'https://images.unsplash.com/photo-1493558103817-58b2924bce98?auto=format&fit=crop&w=1200&q=80',
      entryFee: 120,
      rating: 4.5,
      reviewCount: 96,
      occupancyPercent: 38,
      openTime: '08:30',
      closeTime: '20:00',
      capacity: 190,
      facilities: ['Aile Alani', 'Otopark', 'Sezlong'],
      latitude: 36.5250,
      longitude: 30.5528,
      description:
          'Sig denizi ve sakin koy yapisiyla cocuklu aileler icin rahat, daha dingin bir gun sunar.',
      hasEntryFee: true,
      isOpen: true,
      sunbedPrice: 180,
      phone: '+90 242 000 00 03',
      instagram: '@phaselisfamily',
      hasBar: false,
      hasWaterSports: false,
      isChildFriendly: true,
      hasPool: false,
      hasRestaurant: false,
      hasWifi: false,
      hasParking: true,
      hasSunbeds: true,
      hasShower: true,
      hasDJ: false,
    ),
    const BeachDto(
      id: 4,
      name: 'Olympos Wave Camp',
      location: 'Olympos',
      address: 'Olympos Sahili, Kumluca',
      imageUrl:
          'https://images.unsplash.com/photo-1473116763249-2faaef81ccda?auto=format&fit=crop&w=1200&q=80',
      entryFee: 0,
      rating: 4.3,
      reviewCount: 73,
      occupancyPercent: 41,
      openTime: '07:30',
      closeTime: '21:00',
      capacity: 140,
      facilities: ['Su Sporlari', 'Bar', 'Kamp'],
      latitude: 36.4013,
      longitude: 30.4734,
      description:
          'Sorf, paddle ve daha dogal bir kiyi deneyimi arayan kullanicilar icin aktif bir sahil noktasi.',
      hasEntryFee: false,
      isOpen: true,
      sunbedPrice: 130,
      phone: '+90 242 000 00 04',
      instagram: '@olymposwavecamp',
      hasBar: true,
      hasWaterSports: true,
      isChildFriendly: false,
      hasPool: false,
      hasRestaurant: true,
      hasWifi: false,
      hasParking: false,
      hasSunbeds: true,
      hasShower: true,
      hasDJ: false,
    ),
    const BeachDto(
      id: 5,
      name: 'Kas Sunset Deck',
      location: 'Kas',
      address: 'Cukurbag Yarimadasi, Kas',
      imageUrl:
          'https://images.unsplash.com/photo-1506953823976-52e1fdc0149a?auto=format&fit=crop&w=1200&q=80',
      entryFee: 220,
      rating: 4.9,
      reviewCount: 261,
      occupancyPercent: 67,
      openTime: '10:00',
      closeTime: '00:00',
      capacity: 160,
      facilities: ['Restoran', 'Bar', 'DJ', 'Gun Batimi'],
      latitude: 36.1931,
      longitude: 29.6386,
      description:
          'Gun batimi manzarasi, ozel platform alanlari ve aksam servisiyle ciftler icin one cikan bir deneyim.',
      hasEntryFee: true,
      isOpen: true,
      sunbedPrice: 300,
      phone: '+90 242 000 00 05',
      website: 'kassunset.example.com',
      instagram: '@kassunsetdeck',
      hasBar: true,
      hasWaterSports: false,
      isChildFriendly: false,
      hasPool: false,
      hasRestaurant: true,
      hasWifi: true,
      hasParking: true,
      hasSunbeds: true,
      hasShower: true,
      hasDJ: true,
    ),
  ];

  Future<BeachListResult> getBeaches() async {
    try {
      final response = await _dio.get('/Beaches');
      return BeachListResult(
        beaches: unwrapListResponse(response, BeachDto.fromJson),
        isMock: false,
      );
    } catch (_) {
      return BeachListResult(beaches: _mockBeaches, isMock: true);
    }
  }

  Future<BeachListResult> searchBeaches(String query) async {
    final trimmed = query.trim();
    if (trimmed.isEmpty) {
      return getBeaches();
    }

    try {
      final response = await _dio.get(
        '/Beaches/search',
        queryParameters: {'q': trimmed},
      );
      return BeachListResult(
        beaches: unwrapListResponse(response, BeachDto.fromJson),
        isMock: false,
      );
    } catch (_) {
      final lower = trimmed.toLowerCase();
      final filtered = _mockBeaches.where((beach) {
        return [
          beach.name,
          beach.location,
          beach.address,
          beach.description,
        ].whereType<String>().any((value) => value.toLowerCase().contains(lower));
      }).toList();

      return BeachListResult(beaches: filtered, isMock: true);
    }
  }

  Future<BeachListResult> filterBeaches(BeachFilter filter) async {
    try {
      final response = await _dio.post('/Beaches/filter', data: filter.toJson());
      return BeachListResult(
        beaches: unwrapListResponse(response, BeachDto.fromJson),
        isMock: false,
      );
    } catch (_) {
      return BeachListResult(
        beaches: _applyFilter(_mockBeaches, filter),
        isMock: true,
      );
    }
  }

  Future<BeachDto?> getBeachById(int id) async {
    try {
      final response = await _dio.get('/Beaches/$id');
      return unwrapResponse<BeachDto>(
        response,
        (json) => BeachDto.fromJson(json as Map<String, dynamic>),
      );
    } catch (_) {
      for (final beach in _mockBeaches) {
        if (beach.id == id) return beach;
      }
      return null;
    }
  }

  List<BeachDto> _applyFilter(List<BeachDto> beaches, BeachFilter filter) {
    final result = beaches.where((beach) {
      final rating = beach.rating ?? 0;

      if (filter.minRating != null && rating < filter.minRating!) return false;
      if (filter.hasBar == true && beach.hasBar != true) return false;
      if (filter.hasWaterSports == true && beach.hasWaterSports != true) {
        return false;
      }
      if (filter.isChildFriendly == true && beach.isChildFriendly != true) {
        return false;
      }
      if (filter.hasPool == true && beach.hasPool != true) return false;
      if (filter.freeEntry == true && beach.hasEntryFee == true) return false;

      return true;
    }).toList();

    switch (filter.sortBy) {
      case 'occupancy':
        result.sort(
          (a, b) => (a.occupancyPercent ?? 0).compareTo(b.occupancyPercent ?? 0),
        );
      case 'rating':
      default:
        result.sort((a, b) => (b.rating ?? 0).compareTo(a.rating ?? 0));
    }

    return result;
  }
}

class BeachListResult {
  const BeachListResult({
    required this.beaches,
    required this.isMock,
  });

  final List<BeachDto> beaches;
  final bool isMock;
}
