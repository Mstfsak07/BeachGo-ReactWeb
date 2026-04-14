import 'dart:async';

import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:gap/gap.dart';

import 'package:beachgo/features/stories/domain/entities/story.dart';
import 'package:beachgo/features/stories/presentation/providers/story_provider.dart';

class StoryViewerScreen extends StatefulWidget {
  const StoryViewerScreen({
    super.key,
    required this.group,
    this.initialIndex = 0,
  });

  final StoryBeachGroup group;
  final int initialIndex;

  @override
  State<StoryViewerScreen> createState() => _StoryViewerScreenState();
}

class _StoryViewerScreenState extends State<StoryViewerScreen> {
  Timer? _timer;
  double _progress = 0;
  late int _currentIndex;

  Story get _currentStory => widget.group.stories[_currentIndex];

  @override
  void initState() {
    super.initState();
    _currentIndex = widget.initialIndex.clamp(0, widget.group.stories.length - 1);
    _startProgress();
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  void _startProgress() {
    _timer?.cancel();
    _progress = 0;

    final duration = _currentStory.isVideo
        ? const Duration(seconds: 8)
        : const Duration(seconds: 5);
    const tick = Duration(milliseconds: 50);
    final totalTicks = duration.inMilliseconds / tick.inMilliseconds;

    _timer = Timer.periodic(tick, (timer) {
      if (!mounted) {
        timer.cancel();
        return;
      }

      setState(() {
        _progress += 1 / totalTicks;
      });

      if (_progress >= 1) {
        _goNext();
      }
    });
  }

  void _goNext() {
    if (_currentIndex >= widget.group.stories.length - 1) {
      Navigator.of(context).maybePop();
      return;
    }

    setState(() {
      _currentIndex += 1;
    });
    _startProgress();
  }

  void _goPrevious() {
    if (_currentIndex <= 0) {
      return;
    }

    setState(() {
      _currentIndex -= 1;
    });
    _startProgress();
  }

  @override
  Widget build(BuildContext context) {
    final story = _currentStory;

    return Scaffold(
      backgroundColor: Colors.black,
      body: GestureDetector(
        onVerticalDragEnd: (details) {
          if ((details.primaryVelocity ?? 0) > 250) {
            Navigator.of(context).maybePop();
          }
        },
        child: SafeArea(
          child: Stack(
            fit: StackFit.expand,
            children: [
              _StoryMedia(story: story),
              Positioned.fill(
                child: Row(
                  children: [
                    Expanded(
                      child: GestureDetector(
                        behavior: HitTestBehavior.opaque,
                        onTap: _goPrevious,
                      ),
                    ),
                    Expanded(
                      child: GestureDetector(
                        behavior: HitTestBehavior.opaque,
                        onTap: _goNext,
                      ),
                    ),
                  ],
                ),
              ),
              Positioned(
                top: 12,
                left: 12,
                right: 12,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        for (var index = 0; index < widget.group.stories.length; index++)
                          Expanded(
                            child: Padding(
                              padding: EdgeInsets.only(
                                right: index == widget.group.stories.length - 1 ? 0 : 4,
                              ),
                              child: LinearProgressIndicator(
                                value: index < _currentIndex
                                    ? 1
                                    : index == _currentIndex
                                        ? _progress
                                        : 0,
                                minHeight: 3,
                                backgroundColor: Colors.white24,
                                valueColor:
                                    const AlwaysStoppedAnimation<Color>(Colors.white),
                              ),
                            ),
                          ),
                      ],
                    ),
                    const Gap(14),
                    Row(
                      children: [
                        CircleAvatar(
                          radius: 20,
                          backgroundImage: story.beachImageUrl.isNotEmpty
                              ? CachedNetworkImageProvider(story.beachImageUrl)
                              : null,
                          child: story.beachImageUrl.isEmpty
                              ? Text(
                                  story.beachName.isNotEmpty
                                      ? story.beachName[0].toUpperCase()
                                      : 'B',
                                )
                              : null,
                        ),
                        const Gap(12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                story.beachName,
                                style: const TextStyle(
                                  color: Colors.white,
                                  fontWeight: FontWeight.w700,
                                  fontSize: 16,
                                ),
                              ),
                              Text(
                                _timeAgoLabel(story.createdAt),
                                style: const TextStyle(color: Colors.white70),
                              ),
                            ],
                          ),
                        ),
                        IconButton(
                          onPressed: () => Navigator.of(context).maybePop(),
                          icon: const Icon(Icons.close, color: Colors.white),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              if (story.caption.isNotEmpty)
                Positioned(
                  left: 20,
                  right: 20,
                  bottom: 32,
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                    decoration: BoxDecoration(
                      color: Colors.black.withAlpha(140),
                      borderRadius: BorderRadius.circular(18),
                    ),
                    child: Text(
                      story.caption,
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 15,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }

  String _timeAgoLabel(DateTime createdAt) {
    final elapsed = DateTime.now().toUtc().difference(createdAt);
    if (elapsed.inHours >= 1) {
      return '${elapsed.inHours}s';
    }
    final minutes = elapsed.inMinutes.clamp(1, 59);
    return '${minutes}d';
  }
}

class _StoryMedia extends StatelessWidget {
  const _StoryMedia({required this.story});

  final Story story;

  @override
  Widget build(BuildContext context) {
    if (story.isVideo) {
      return Container(
        color: Colors.black,
        alignment: Alignment.center,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(
              Icons.play_circle_fill_rounded,
              color: Colors.white,
              size: 88,
            ),
            const Gap(16),
            Text(
              'Video story',
              style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.w800,
                  ),
            ),
            const Gap(8),
            const Text(
              'Minimal surumde video oynatici yerine guvenli placeholder gosteriliyor.',
              textAlign: TextAlign.center,
              style: TextStyle(color: Colors.white70),
            ),
          ],
        ),
      );
    }

    return CachedNetworkImage(
      imageUrl: story.mediaUrl,
      fit: BoxFit.cover,
      errorWidget: (_, __, ___) => Container(
        color: Colors.black,
        alignment: Alignment.center,
        child: const Icon(
          Icons.broken_image_outlined,
          color: Colors.white54,
          size: 80,
        ),
      ),
    );
  }
}
