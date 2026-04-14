import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:gap/gap.dart';

import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/features/stories/presentation/providers/story_provider.dart';
import 'package:beachgo/features/stories/presentation/screens/story_viewer_screen.dart';

class StoryStrip extends ConsumerWidget {
  const StoryStrip({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final storiesAsync = ref.watch(groupedStoriesProvider);

    return storiesAsync.when(
      data: (result) => switch (result) {
        Success<List<StoryBeachGroup>>(data: final groups) => groups.isEmpty
            ? const SizedBox.shrink()
            : Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Storyler',
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                  const Gap(12),
                  SizedBox(
                    height: 104,
                    child: ListView.separated(
                      scrollDirection: Axis.horizontal,
                      itemCount: groups.length,
                      separatorBuilder: (_, __) => const Gap(14),
                      itemBuilder: (context, index) {
                        final group = groups[index];
                        return _StoryAvatar(
                          group: group,
                          onTap: () {
                            Navigator.of(context).push(
                              MaterialPageRoute<void>(
                                builder: (_) => StoryViewerScreen(group: group),
                                fullscreenDialog: true,
                              ),
                            );
                          },
                        );
                      },
                    ),
                  ),
                ],
              ),
        FailureResult<List<StoryBeachGroup>>() => const SizedBox.shrink(),
      },
      loading: () => const SizedBox(
        height: 104,
        child: _StoryStripSkeleton(),
      ),
      error: (_, __) => const SizedBox.shrink(),
    );
  }
}

class _StoryAvatar extends StatelessWidget {
  const _StoryAvatar({
    required this.group,
    required this.onTap,
  });

  final StoryBeachGroup group;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final imageProvider = group.coverImageUrl.isNotEmpty
        ? CachedNetworkImageProvider(group.coverImageUrl)
        : null;

    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(999),
      child: SizedBox(
        width: 78,
        child: Column(
          children: [
            Container(
              padding: const EdgeInsets.all(3),
              decoration: const BoxDecoration(
                shape: BoxShape.circle,
                gradient: LinearGradient(
                  colors: [Color(0xFFF97316), Color(0xFFEC4899), Color(0xFF8B5CF6)],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
              ),
              child: CircleAvatar(
                radius: 30,
                backgroundColor: Colors.white,
                child: CircleAvatar(
                  radius: 27,
                  backgroundImage: imageProvider,
                  child: imageProvider == null
                      ? Text(
                          group.beachName.isNotEmpty
                              ? group.beachName[0].toUpperCase()
                              : 'B',
                        )
                      : null,
                ),
              ),
            ),
            const Gap(8),
            Text(
              group.beachName,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ],
        ),
      ),
    );
  }
}

class _StoryStripSkeleton extends StatelessWidget {
  const _StoryStripSkeleton();

  @override
  Widget build(BuildContext context) {
    return ListView.separated(
      scrollDirection: Axis.horizontal,
      itemCount: 5,
      separatorBuilder: (_, __) => const Gap(14),
      itemBuilder: (_, __) => SizedBox(
        width: 78,
        child: Column(
          children: [
            Container(
              width: 66,
              height: 66,
              decoration: const BoxDecoration(
                shape: BoxShape.circle,
                color: Color(0xFFE5E7EB),
              ),
            ),
            const Gap(8),
            Container(
              width: 58,
              height: 10,
              decoration: BoxDecoration(
                color: const Color(0xFFE5E7EB),
                borderRadius: BorderRadius.circular(999),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
