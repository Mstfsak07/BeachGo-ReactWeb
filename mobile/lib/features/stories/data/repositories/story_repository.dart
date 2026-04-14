import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/core/network/api_response.dart';
import 'package:beachgo/core/network/dio_client.dart';
import 'package:beachgo/features/stories/data/models/create_story_request_dto.dart';
import 'package:beachgo/features/stories/data/models/story_dto.dart';
import 'package:beachgo/features/stories/domain/entities/story.dart';

final storyRepositoryProvider = Provider<StoryRepository>((ref) {
  return StoryRepositoryImpl(ref.watch(dioProvider));
});

abstract class StoryRepository {
  Future<Result<List<Story>>> getActiveStories();

  Future<Result<List<Story>>> getBeachStories(int beachId);

  Future<Result<Story>> createStory({
    required int beachId,
    required String mediaUrl,
    required String mediaType,
    required String caption,
    required int expireHours,
  });
}

class StoryRepositoryImpl implements StoryRepository {
  StoryRepositoryImpl(this._dio);

  final Dio _dio;

  @override
  Future<Result<List<Story>>> getActiveStories() {
    return _execute<List<Story>>(
      request: () => _dio.get('/Stories'),
      parser: _parseStoryList,
    );
  }

  @override
  Future<Result<List<Story>>> getBeachStories(int beachId) {
    return _execute<List<Story>>(
      request: () => _dio.get('/Stories/beach/$beachId'),
      parser: _parseStoryList,
    );
  }

  @override
  Future<Result<Story>> createStory({
    required int beachId,
    required String mediaUrl,
    required String mediaType,
    required String caption,
    required int expireHours,
  }) {
    return _execute<Story>(
      request: () => _dio.post(
        '/Stories',
        data: CreateStoryRequestDto(
          beachId: beachId,
          mediaUrl: mediaUrl,
          mediaType: mediaType,
          caption: caption,
          expireHours: expireHours,
        ).toJson(),
      ),
      parser: (raw) => StoryDto.fromJson(_asMap(raw)).toDomain(),
    );
  }

  Future<Result<T>> _execute<T>({
    required Future<Response<dynamic>> Function() request,
    required T Function(Object? raw) parser,
  }) async {
    try {
      final response = await request();
      final responseData = response.data;
      if (responseData is! Map<String, dynamic>) {
        return FailureResult<T>(const ServerFailure('Invalid response format.'));
      }

      final apiResponse = ApiResponse<T>.fromJson(responseData, parser);
      if (!apiResponse.success) {
        final message = apiResponse.message.isNotEmpty
            ? apiResponse.message
            : apiResponse.errors.join(', ');
        return FailureResult<T>(
          _mapStatusToFailure(
            response.statusCode,
            message.isNotEmpty ? message : 'Request failed on server.',
          ),
        );
      }

      final data = apiResponse.data;
      if (data == null) {
        return FailureResult<T>(const NotFoundFailure('Response data is empty.'));
      }

      return Success<T>(data);
    } on DioException catch (error) {
      return FailureResult<T>(_mapDioException(error));
    } catch (_) {
      return FailureResult<T>(const UnknownFailure());
    }
  }

  List<Story> _parseStoryList(Object? raw) {
    if (raw is! List) {
      return const <Story>[];
    }

    return raw
        .whereType<Map<String, dynamic>>()
        .map(StoryDto.fromJson)
        .map((dto) => dto.toDomain())
        .where(_isValidStory)
        .toList(growable: false);
  }

  bool _isValidStory(Story story) {
    final uri = Uri.tryParse(story.mediaUrl);
    if (uri == null || !uri.hasScheme) {
      return false;
    }

    return (uri.scheme == 'http' || uri.scheme == 'https') &&
        story.expiresAt.isAfter(DateTime.now().toUtc());
  }

  Failure _mapStatusToFailure(int? statusCode, String message) {
    if (statusCode == 401 || statusCode == 403) {
      return UnauthorizedFailure(message);
    }
    if (statusCode == 404) {
      return NotFoundFailure(message);
    }
    if (statusCode != null && statusCode >= 500) {
      return ServerFailure(message);
    }
    return ServerFailure(message);
  }

  Failure _mapDioException(DioException error) {
    final statusCode = error.response?.statusCode;
    final message = _extractErrorMessage(error);

    if (statusCode == 401 || statusCode == 403) {
      return UnauthorizedFailure(message);
    }
    if (statusCode == 404) {
      return NotFoundFailure(message);
    }
    if (statusCode != null && statusCode >= 500) {
      return ServerFailure(message);
    }
    if (error.type == DioExceptionType.connectionTimeout ||
        error.type == DioExceptionType.receiveTimeout ||
        error.type == DioExceptionType.sendTimeout ||
        error.type == DioExceptionType.connectionError) {
      return NetworkFailure(message);
    }
    return UnknownFailure(message);
  }

  String _extractErrorMessage(DioException error) {
    final data = error.response?.data;
    if (data is Map<String, dynamic>) {
      final apiResponse = ApiResponse<Object?>.fromJson(data, (raw) => raw);
      if (apiResponse.message.isNotEmpty) return apiResponse.message;
      if (apiResponse.errors.isNotEmpty) return apiResponse.errors.join(', ');
    }
    return error.message ?? 'Request failed.';
  }

  Map<String, dynamic> _asMap(Object? raw) {
    if (raw is Map<String, dynamic>) {
      return raw;
    }
    return const <String, dynamic>{};
  }
}
