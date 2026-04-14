// This is a basic Flutter widget test.
//
// To perform an interaction with a widget in your test, use the WidgetTester
// utility in the flutter_test package. For example, you can send tap and scroll
// gestures. You can also use WidgetTester to find child widgets in the widget
// tree, read text, and verify that the values of widget properties are correct.

import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:beachgo/core/models/models.dart';
import 'package:beachgo/features/beach/data/beach_repository.dart';
import 'package:beachgo/main.dart';

void main() {
  testWidgets('app boots', (WidgetTester tester) async {
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          beachRepositoryProvider.overrideWithValue(_FakeBeachRepository()),
        ],
        child: const BeachGoApp(),
      ),
    );

    await tester.pumpAndSettle();

    expect(find.byType(MaterialApp), findsOneWidget);
    expect(find.text('Plajlari Kesfet'), findsOneWidget);
  });
}

class _FakeBeachRepository extends BeachRepository {
  _FakeBeachRepository() : super(Dio());

  @override
  Future<BeachListResult> getBeaches() async {
    return const BeachListResult(
      beaches: [
        BeachDto(
          id: 1,
          name: 'Test Beach',
          address: 'Antalya',
          rating: 4.5,
        ),
      ],
      isMock: true,
    );
  }

  @override
  Future<BeachListResult> searchBeaches(String query) => getBeaches();

  @override
  Future<BeachListResult> filterBeaches(BeachFilter filter) => getBeaches();

  @override
  Future<BeachDto?> getBeachById(int id) async {
    return const BeachDto(id: 1, name: 'Test Beach');
  }
}
