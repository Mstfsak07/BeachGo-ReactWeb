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

class _StoryViewerScreenState extends State<StoryViewerScreen>
    with SingleTickerProviderStateMixin {
  static const _imageStoryDuration = Duration(seconds: 5);
  static const _videoStoryDuration = Duration(seconds: 7);
  static const _dismissThreshold = 120.0;

  late final AnimationController _progressController;
  final Set<int> _failedStoryIds = <int>{};

  late int _currentIndex;
  bool _isPaused = false;
  bool _isCurrentStoryReady = false;
  double _dragOffsetY = 0;

  Story get _currentStory => widget.group.stories[_currentIndex];
  bool get _hasStories => widget.group.stories.isNotEmpty;

  @override
  void initState() {
    super.initState();
    _progressController = AnimationController(vsync: this)
      ..addStatusListener(_handleProgressStatusChanged);

    if (!_hasStories) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (mounted) {
          Navigator.of(context).maybePop();
        }
      });
      _currentIndex = 0;
      return;
    }

    _currentIndex = widget.initialIndex.clamp(0, widget.group.stories.length - 1);
    _configureCurrentStory();
  }

  @override
  void dispose() {
    _progressController
      ..removeStatusListener(_handleProgressStatusChanged)
      ..dispose();
    super.dispose();
  }

  void _handleProgressStatusChanged(AnimationStatus status) {
    if (status == AnimationStatus.completed) {
      _goNext();
    }
  }

  void _configureCurrentStory() {
    _isCurrentStoryReady = _currentStory.isVideo;
    _progressController
      ..stop()
      ..duration = _currentStory.isVideo ? _videoStoryDuration : _imageStoryDuration
      ..value = 0;

    if (_isCurrentStoryReady && !_isPaused) {
      _progressController.forward();
    }
  }

  void _goNext() {
    if (!_hasStories) {
      Navigator.of(context).maybePop();
      return;
    }

    if (_currentIndex >= widget.group.stories.length - 1) {
      Navigator.of(context).maybePop();
      return;
    }

    setState(() {
      _currentIndex += 1;
      _dragOffsetY = 0;
    });
    _configureCurrentStory();
  }

  void _goPrevious() {
    if (!_hasStories || _currentIndex <= 0) {
      Navigator.of(context).maybePop();
      return;
    }

    setState(() {
      _currentIndex -= 1;
      _dragOffsetY = 0;
    });
    _configureCurrentStory();
  }

  void _pauseStory() {
    if (_isPaused) {
      return;
    }

    _isPaused = true;
    _progressController.stop(canceled: false);
  }

  void _resumeStory() {
    if (!_isPaused) {
      return;
    }

    _isPaused = false;
    if (_isCurrentStoryReady) {
      _progressController.forward();
    }
  }

  void _handleStoryLoaded() {
    if (!mounted || _isCurrentStoryReady) {
      return;
    }

    setState(() {
      _isCurrentStoryReady = true;
    });

    if (!_isPaused) {
      _progressController.forward();
    }
  }

  void _handleStoryFailed() {
    if (!_hasStories) {
      Navigator.of(context).maybePop();
      return;
    }

    final storyId = _currentStory.id;
    if (_failedStoryIds.contains(storyId)) {
      return;
    }
    _failedStoryIds.add(storyId);

    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) {
        return;
      }

      if (_currentIndex >= widget.group.stories.length - 1) {
        Navigator.of(context).maybePop();
        return;
      }

      _goNext();
    });
  }

  void _handleVerticalDragUpdate(DragUpdateDetails details) {
    final nextOffset = _dragOffsetY + details.delta.dy;
    if (nextOffset < 0) {
      return;
    }

    if (_dragOffsetY == 0) {
      _pauseStory();
    }

    setState(() {
      _dragOffsetY = nextOffset;
    });
  }

  void _handleVerticalDragEnd(DragEndDetails details) {
    final velocity = details.primaryVelocity ?? 0;
    if (_dragOffsetY > _dismissThreshold || velocity > 900) {
      Navigator.of(context).maybePop();
      return;
    }

    setState(() {
      _dragOffsetY = 0;
    });
    _resumeStory();
  }

  @override
  Widget build(BuildContext context) {
    if (!_hasStories) {
      return const Scaffold(backgroundColor: Colors.black);
    }

    final story = _currentStory;
    final overlayOpacity = (1 - (_dragOffsetY / 320)).clamp(0.65, 1.0);

    return Scaffold(
      backgroundColor: Colors.black,
      body: GestureDetector(
        behavior: HitTestBehavior.opaque,
        onLongPressStart: (_) => _pauseStory(),
        onLongPressEnd: (_) => _resumeStory(),
        onVerticalDragUpdate: _handleVerticalDragUpdate,
        onVerticalDragEnd: _handleVerticalDragEnd,
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 180),
          curve: Curves.easeOut,
          transform: Matrix4.translationValues(0, _dragOffsetY, 0),
          child: ColoredBox(
            color: Colors.black.withValues(alpha: overlayOpacity),
            child: SafeArea(
              bottom: false,
              child: Stack(
                fit: StackFit.expand,
                children: [
                  AnimatedSwitcher(
                    duration: const Duration(milliseconds: 250),
                    switchInCurve: Curves.easeOut,
                    switchOutCurve: Curves.easeIn,
                    transitionBuilder: (child, animation) {
                      return FadeTransition(opacity: animation, child: child);
                    },
                    child: _StoryMedia(
                      key: ValueKey<int>(story.id),
                      story: story,
                      onLoaded: _handleStoryLoaded,
                      onFailed: _handleStoryFailed,
                    ),
                  ),
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
                    child: _StoryTopOverlay(
                      group: widget.group,
                      story: story,
                      currentIndex: _currentIndex,
                      progress: _progressController,
                      onClose: () => Navigator.of(context).maybePop(),
                    ),
                  ),
                  if (story.caption.isNotEmpty)
                    Positioned(
                      left: 20,
                      right: 20,
                      bottom: 32,
                      child: IgnorePointer(
                        child: Text(
                          story.caption,
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 16,
                            fontWeight: FontWeight.w600,
                            shadows: [
                              Shadow(
                                color: Colors.black87,
                                blurRadius: 14,
                                offset: Offset(0, 2),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _StoryTopOverlay extends StatelessWidget {
  const _StoryTopOverlay({
    required this.group,
    required this.story,
    required this.currentIndex,
    required this.progress,
    required this.onClose,
  });

  final StoryBeachGroup group;
  final Story story;
  final int currentIndex;
  final Animation<double> progress;
  final VoidCallback onClose;

  @override
  Widget build(BuildContext context) {
    const shadow = [
      Shadow(
        color: Colors.black87,
        blurRadius: 16,
        offset: Offset(0, 2),
      ),
    ];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _StoryProgressBars(
          storyCount: group.stories.length,
          currentIndex: currentIndex,
          progress: progress,
        ),
        const Gap(14),
        Row(
          children: [
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    story.beachName,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 18,
                      fontWeight: FontWeight.w700,
                      shadows: shadow,
                    ),
                  ),
                  const Gap(4),
                  Text(
                    _timeAgoLabel(story.createdAt),
                    style: const TextStyle(
                      color: Colors.white70,
                      fontSize: 13,
                      fontWeight: FontWeight.w500,
                      shadows: shadow,
                    ),
                  ),
                ],
              ),
            ),
            IconButton(
              onPressed: onClose,
              icon: const Icon(Icons.close_rounded, color: Colors.white),
            ),
          ],
        ),
      ],
    );
  }

  static String _timeAgoLabel(DateTime createdAt) {
    final elapsed = DateTime.now().toUtc().difference(createdAt);
    if (elapsed.inHours >= 1) {
      return '${elapsed.inHours}s';
    }
    final minutes = elapsed.inMinutes.clamp(1, 59);
    return '${minutes}d';
  }
}

class _StoryProgressBars extends StatelessWidget {
  const _StoryProgressBars({
    required this.storyCount,
    required this.currentIndex,
    required this.progress,
  });

  final int storyCount;
  final int currentIndex;
  final Animation<double> progress;

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: progress,
      builder: (context, _) {
        return Row(
          children: List<Widget>.generate(storyCount, (index) {
            final value = switch (index.compareTo(currentIndex)) {
              -1 => 1.0,
              0 => progress.value,
              _ => 0.0,
            };

            return Expanded(
              child: Padding(
                padding: EdgeInsets.only(right: index == storyCount - 1 ? 0 : 4),
                child: _StoryProgressBar(value: value),
              ),
            );
          }),
        );
      },
    );
  }
}

class _StoryProgressBar extends StatelessWidget {
  const _StoryProgressBar({required this.value});

  final double value;

  @override
  Widget build(BuildContext context) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(999),
      child: LinearProgressIndicator(
        value: value,
        minHeight: 3,
        backgroundColor: Colors.white24,
        valueColor: const AlwaysStoppedAnimation<Color>(Colors.white),
      ),
    );
  }
}

class _StoryMedia extends StatefulWidget {
  const _StoryMedia({
    super.key,
    required this.story,
    required this.onLoaded,
    required this.onFailed,
  });

  final Story story;
  final VoidCallback onLoaded;
  final VoidCallback onFailed;

  @override
  State<_StoryMedia> createState() => _StoryMediaState();
}

class _StoryMediaState extends State<_StoryMedia> {
  bool _handledReady = false;
  bool _handledFailure = false;

  @override
  void didUpdateWidget(covariant _StoryMedia oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.story.id != widget.story.id) {
      _handledReady = false;
      _handledFailure = false;
    }
  }

  void _notifyLoaded() {
    if (_handledReady) {
      return;
    }
    _handledReady = true;
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        widget.onLoaded();
      }
    });
  }

  void _notifyFailed() {
    if (_handledFailure) {
      return;
    }
    _handledFailure = true;
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        widget.onFailed();
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    if (widget.story.isVideo) {
      _notifyLoaded();
      return _VideoStoryPlaceholder(story: widget.story);
    }

    return CachedNetworkImage(
      imageUrl: widget.story.mediaUrl,
      fit: BoxFit.cover,
      imageBuilder: (context, imageProvider) {
        _notifyLoaded();
        return DecoratedBox(
          decoration: BoxDecoration(
            image: DecorationImage(
              image: imageProvider,
              fit: BoxFit.cover,
            ),
          ),
        );
      },
      progressIndicatorBuilder: (context, _, __) {
        return const ColoredBox(
          color: Colors.black,
          child: Center(
            child: SizedBox(
              width: 28,
              height: 28,
              child: CircularProgressIndicator(
                strokeWidth: 2.2,
                valueColor: AlwaysStoppedAnimation<Color>(Colors.white70),
              ),
            ),
          ),
        );
      },
      errorWidget: (context, _, __) {
        _notifyFailed();
        return const ColoredBox(color: Colors.black);
      },
    );
  }
}

class _VideoStoryPlaceholder extends StatelessWidget {
  const _VideoStoryPlaceholder({required this.story});

  final Story story;

  @override
  Widget build(BuildContext context) {
    final imageUrl = story.beachImageUrl.isNotEmpty ? story.beachImageUrl : story.mediaUrl;

    return Stack(
      fit: StackFit.expand,
      children: [
        CachedNetworkImage(
          imageUrl: imageUrl,
          fit: BoxFit.cover,
          errorWidget: (_, __, ___) => const ColoredBox(color: Colors.black),
        ),
        Container(color: Colors.black45),
        const Center(
          child: Icon(
            Icons.play_circle_fill_rounded,
            color: Colors.white,
            size: 88,
          ),
        ),
      ],
    );
  }
}
