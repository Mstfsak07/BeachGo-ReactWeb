import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/features/stories/data/repositories/story_repository.dart';
import 'package:beachgo/features/stories/domain/entities/story.dart';

final activeStoriesProvider = FutureProvider<Result<List<Story>>>((ref) {
  return ref.watch(storyRepositoryProvider).getActiveStories();
});

final beachStoriesProvider =
    FutureProvider.family<Result<List<Story>>, int>((ref, beachId) {
  return ref.watch(storyRepositoryProvider).getBeachStories(beachId);
});

final groupedStoriesProvider =
    FutureProvider<Result<List<StoryBeachGroup>>>((ref) async {
  final result = await ref.watch(storyRepositoryProvider).getActiveStories();

  return switch (result) {
    Success<List<Story>>(data: final stories) => Success(_groupStories(stories)),
    FailureResult<List<Story>>(failure: final failure) => FailureResult(failure),
  };
});

List<StoryBeachGroup> _groupStories(List<Story> stories) {
  final grouped = <int, List<Story>>{};

  for (final story in stories) {
    grouped.putIfAbsent(story.beachId, () => <Story>[]).add(story);
  }

  return grouped.entries
      .map((entry) {
        final beachStories = entry.value
          ..sort((a, b) => a.createdAt.compareTo(b.createdAt));
        final coverStory = beachStories.first;
        return StoryBeachGroup(
          beachId: entry.key,
          beachName: coverStory.beachName,
          coverImageUrl: coverStory.beachImageUrl.isNotEmpty
              ? coverStory.beachImageUrl
              : coverStory.mediaUrl,
          stories: List<Story>.unmodifiable(beachStories),
        );
      })
      .where((group) => group.stories.isNotEmpty)
      .toList(growable: false)
    ..sort((a, b) => b.stories.last.createdAt.compareTo(a.stories.last.createdAt));
}

class StoryBeachGroup {
  const StoryBeachGroup({
    required this.beachId,
    required this.beachName,
    required this.coverImageUrl,
    required this.stories,
  });

  final int beachId;
  final String beachName;
  final String coverImageUrl;
  final List<Story> stories;
}
