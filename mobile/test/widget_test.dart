// This is a basic Flutter widget test.
//
// To perform an interaction with a widget in your test, use the WidgetTester
// utility in the flutter_test package. For example, you can send tap and scroll
// gestures. You can also use WidgetTester to find child widgets in the widget
// tree, read text, and verify that the values of widget properties are correct.

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/core/network/paged_response.dart';
import 'package:beachgo/features/app/presentation/screens/main_shell_screen.dart';
import 'package:beachgo/features/auth/data/repositories/auth_repository.dart';
import 'package:beachgo/features/auth/domain/entities/app_user.dart';
import 'package:beachgo/features/auth/domain/entities/auth_session.dart';
import 'package:beachgo/features/beach/data/repository/beach_repository.dart';
import 'package:beachgo/features/beach/domain/entities/beach.dart';
import 'package:beachgo/features/beach/domain/entities/beach_filter.dart';
import 'package:beachgo/features/beach/domain/entities/beach_review.dart';
import 'package:beachgo/features/beach/domain/entities/weather.dart';
import 'package:beachgo/features/stories/data/repositories/story_repository.dart';
import 'package:beachgo/features/stories/domain/entities/story.dart';
import 'package:beachgo/main.dart';

void main() {
  testWidgets('app boots', (WidgetTester tester) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          authRepositoryProvider.overrideWithValue(_FakeAuthRepository()),
          beachRepositoryProvider.overrideWithValue(_FakeBeachRepository()),
          storyRepositoryProvider.overrideWithValue(_FakeStoryRepository()),
        ],
        child: const BeachGoApp(),
      ),
    );

    await tester.pump();
    await tester.pump(const Duration(milliseconds: 200));

    expect(find.byType(MaterialApp), findsOneWidget);
    expect(find.text('Plajlari Kesfet'), findsOneWidget);
    expect(find.text('Kesfet'), findsOneWidget);
  });

  testWidgets('main shell renders bottom navigation', (WidgetTester tester) async {
    await tester.pumpWidget(
      const ProviderScope(
        child: MaterialApp(
          home: MainShellScreen(currentTab: AppShellTab.favorites),
        ),
      ),
    );

    expect(find.text('Favoriler'), findsWidgets);
    expect(find.text('Kesfet'), findsOneWidget);
    expect(find.text('Profil'), findsOneWidget);
  });
}

class _FakeAuthRepository extends AuthRepository {
  @override
  Future<void> clearSession() async {}

  @override
  Future<Result<AppUser?>> getStoredUser() async {
    return const Success<AppUser?>(null);
  }

  @override
  Future<Result<AuthSession>> login({
    required String email,
    required String password,
  }) async {
    return Success(
      AuthSession(
        accessToken: 'token',
        refreshToken: 'refresh',
        user: AppUser(
          id: 1,
          email: email,
          role: 'User',
          accountType: 'User',
          firstName: 'Test',
          lastName: 'User',
          name: 'Test User',
          phone: '',
        ),
      ),
    );
  }

  @override
  Future<void> persistSession(AuthSession session) async {}

  @override
  Future<Result<AuthSession>> refreshSession() async {
    return const FailureResult<AuthSession>(
      UnauthorizedFailure('No session'),
    );
  }

  @override
  Future<Result<void>> register({
    required String name,
    required String email,
    required String password,
  }) async {
    return const Success<void>(null);
  }

  @override
  Future<Result<void>> businessRegister({
    required String businessName,
    required String contactName,
    required String email,
    required String password,
    String phoneNumber = '',
  }) async {
    return const Success<void>(null);
  }

  @override
  Future<void> updateStoredUser(AppUser user) async {}
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

  @override
  Future<Result<List<BeachReview>>> getBeachReviews(int beachId) async {
    return const Success(<BeachReview>[]);
  }

  @override
  Future<Result<void>> createReview({
    required int beachId,
    required String userName,
    required String userPhone,
    required int rating,
    required String comment,
  }) async {
    return const Success<void>(null);
  }

  @override
  Future<Result<List<Beach>>> getFavoriteBeaches() async {
    return const Success(<Beach>[]);
  }

  @override
  Future<Result<void>> addFavorite(int beachId) async {
    return const Success<void>(null);
  }

  @override
  Future<Result<void>> removeFavorite(int beachId) async {
    return const Success<void>(null);
  }
}
