// This is a basic Flutter widget test.
//
// To perform an interaction with a widget in your test, use the WidgetTester
// utility in the flutter_test package. For example, you can send tap and scroll
// gestures. You can also use WidgetTester to find child widgets in the widget
// tree, read text, and verify that the values of widget properties are correct.

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/core/network/paged_response.dart';
import 'package:beachgo/features/beach/data/repository/beach_repository.dart';
import 'package:beachgo/features/beach/domain/entities/beach.dart';
import 'package:beachgo/features/beach/domain/entities/beach_filter.dart';
import 'package:beachgo/features/beach/domain/entities/weather.dart';
import 'package:beachgo/features/stories/data/repositories/story_repository.dart';
import 'package:beachgo/features/stories/domain/entities/story.dart';
import 'package:beachgo/main.dart';

void main() {
  testWidgets('app boots', (WidgetTester tester) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          beachRepositoryProvider.overrideWithValue(_FakeBeachRepository()),
          storyRepositoryProvider.overrideWithValue(_FakeStoryRepository()),
        ],
        child: const BeachGoApp(),
      ),
    );

    await tester.pumpAndSettle();

    expect(find.byType(MaterialApp), findsOneWidget);
    expect(find.text('Plajlari Kesfet'), findsOneWidget);
  });
}

class _FakeStoryRepository extends StoryRepository {
  @override
  Future<Result<Story>> createStory({
    required int beachId,
    required String mediaUrl,
    required String mediaType,
    required String caption,
    required int expireHours,
  }) async {
    return Success(
      Story(
        id: 1,
        beachId: beachId,
        beachName: 'Test Beach',
        beachImageUrl: '',
        mediaUrl: mediaUrl,
        mediaType: mediaType,
        caption: caption,
        createdAt: DateTime.now().toUtc(),
        expiresAt: DateTime.now().toUtc().add(Duration(hours: expireHours)),
      ),
    );
  }

  @override
  Future<Result<List<Story>>> getActiveStories() async {
    return const Success(<Story>[]);
  }

  @override
  Future<Result<List<Story>>> getBeachStories(int beachId) async {
    return const Success(<Story>[]);
  }
}

class _FakeBeachRepository extends BeachRepository {
  @override
  Future<Result<PagedResponse<Beach>>> getBeaches({
    int page = 1,
    int pageSize = 20,
  }) async {
    return const Success(
      PagedResponse(
        items: [
          Beach(
            id: 1,
            name: 'Test Beach',
            location: '',
            address: 'Antalya',
            imageUrl: '',
            entryFee: 0,
            rating: 4.5,
            reviewCount: 0,
            occupancyPercent: 0,
            openTime: '',
            closeTime: '',
            capacity: 0,
            facilities: [],
            latitude: null,
            longitude: null,
            description: '',
            hasEntryFee: false,
            isOpen: true,
            sunbedPrice: 0,
            phone: '',
            website: '',
            instagram: '',
            hasBar: false,
            hasWaterSports: false,
            isChildFriendly: false,
            hasPool: false,
            hasRestaurant: false,
            hasWifi: false,
            hasParking: false,
            hasSunbeds: false,
            hasShower: false,
            hasDJ: false,
          ),
        ],
        page: 1,
        pageSize: 20,
        totalCount: 1,
      ),
    );
  }

  @override
  Future<Result<List<Beach>>> searchBeaches(String query) async {
    final response = await getBeaches();
    final page = (response as Success<PagedResponse<Beach>>).data;
    return Success(page.items);
  }

  @override
  Future<Result<List<Beach>>> filterBeaches(BeachFilter filter) =>
      searchBeaches('');

  @override
  Future<Result<Beach>> getBeachById(int id) async {
    return const Success(
      Beach(
        id: 1,
        name: 'Test Beach',
        location: '',
        address: 'Antalya',
        imageUrl: '',
        entryFee: 0,
        rating: 4.5,
        reviewCount: 0,
        occupancyPercent: 0,
        openTime: '',
        closeTime: '',
        capacity: 0,
        facilities: [],
        latitude: null,
        longitude: null,
        description: '',
        hasEntryFee: false,
        isOpen: true,
        sunbedPrice: 0,
        phone: '',
        website: '',
        instagram: '',
        hasBar: false,
        hasWaterSports: false,
        isChildFriendly: false,
        hasPool: false,
        hasRestaurant: false,
        hasWifi: false,
        hasParking: false,
        hasSunbeds: false,
        hasShower: false,
        hasDJ: false,
      ),
    );
  }

  @override
  Future<Result<Weather>> getBeachWeather(int id) async {
    return const Success(
      Weather(
        temperature: null,
        description: '',
        windSpeed: null,
        seaTemperature: null,
        waveHeight: null,
      ),
    );
  }
}
