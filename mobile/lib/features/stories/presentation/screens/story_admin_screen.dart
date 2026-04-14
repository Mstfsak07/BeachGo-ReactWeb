import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:gap/gap.dart';

import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/features/stories/data/repositories/story_repository.dart';
import 'package:beachgo/features/stories/presentation/providers/story_provider.dart';
import 'package:beachgo/shared/widgets/shared_widgets.dart';

class StoryAdminScreen extends ConsumerStatefulWidget {
  const StoryAdminScreen({super.key});

  @override
  ConsumerState<StoryAdminScreen> createState() => _StoryAdminScreenState();
}

class _StoryAdminScreenState extends ConsumerState<StoryAdminScreen> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _beachIdController;
  late final TextEditingController _mediaUrlController;
  late final TextEditingController _captionController;
  late final TextEditingController _expireHoursController;

  String _mediaType = 'image';
  bool _isSubmitting = false;

  @override
  void initState() {
    super.initState();
    _beachIdController = TextEditingController();
    _mediaUrlController = TextEditingController();
    _captionController = TextEditingController();
    _expireHoursController = TextEditingController(text: '24');
  }

  @override
  void dispose() {
    _beachIdController.dispose();
    _mediaUrlController.dispose();
    _captionController.dispose();
    _expireHoursController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    final form = _formKey.currentState;
    if (form == null || !form.validate() || _isSubmitting) {
      return;
    }

    setState(() {
      _isSubmitting = true;
    });

    final result = await ref.read(storyRepositoryProvider).createStory(
          beachId: int.parse(_beachIdController.text.trim()),
          mediaUrl: _mediaUrlController.text.trim(),
          mediaType: _mediaType,
          caption: _captionController.text.trim(),
          expireHours: int.parse(_expireHoursController.text.trim()),
        );

    if (!mounted) {
      return;
    }

    setState(() {
      _isSubmitting = false;
    });

    switch (result) {
      case Success():
        ref.invalidate(activeStoriesProvider);
        ref.invalidate(groupedStoriesProvider);
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Story olusturuldu.')),
        );
        _mediaUrlController.clear();
        _captionController.clear();
        _expireHoursController.text = '24';
      case FailureResult(failure: final failure):
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(failure.message)),
        );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Story Ekle')),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(20),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Manuel Story Girisi',
                  style: Theme.of(context).textTheme.headlineMedium,
                ),
                const Gap(8),
                Text(
                  'Minimal surum: URL ile image/video story ekle.',
                  style: Theme.of(context).textTheme.bodyMedium,
                ),
                const Gap(24),
                TextFormField(
                  controller: _beachIdController,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(
                    labelText: 'Beach ID',
                    hintText: 'Ornek: 12',
                  ),
                  validator: (value) {
                    final parsed = int.tryParse(value?.trim() ?? '');
                    if (parsed == null || parsed <= 0) {
                      return 'Gecerli bir beach id girin.';
                    }
                    return null;
                  },
                ),
                const Gap(16),
                DropdownButtonFormField<String>(
                  initialValue: _mediaType,
                  decoration: const InputDecoration(labelText: 'Medya Turu'),
                  items: const [
                    DropdownMenuItem(value: 'image', child: Text('Image')),
                    DropdownMenuItem(value: 'video', child: Text('Video')),
                  ],
                  onChanged: (value) {
                    if (value == null) return;
                    setState(() {
                      _mediaType = value;
                    });
                  },
                ),
                const Gap(16),
                TextFormField(
                  controller: _mediaUrlController,
                  decoration: const InputDecoration(
                    labelText: 'Media URL',
                    hintText: 'https://...',
                  ),
                  validator: (value) {
                    final uri = Uri.tryParse(value?.trim() ?? '');
                    if (uri == null || !uri.hasScheme) {
                      return 'Gecerli bir URL girin.';
                    }
                    if (uri.scheme != 'http' && uri.scheme != 'https') {
                      return 'URL http veya https olmali.';
                    }
                    return null;
                  },
                ),
                const Gap(16),
                TextFormField(
                  controller: _captionController,
                  maxLines: 3,
                  decoration: const InputDecoration(
                    labelText: 'Caption',
                    hintText: 'Istege bagli',
                  ),
                ),
                const Gap(16),
                TextFormField(
                  controller: _expireHoursController,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(
                    labelText: 'Expiration (hours)',
                    hintText: '24',
                  ),
                  validator: (value) {
                    final parsed = int.tryParse(value?.trim() ?? '');
                    if (parsed == null || parsed < 1 || parsed > 168) {
                      return '1 ile 168 saat arasinda bir deger girin.';
                    }
                    return null;
                  },
                ),
                const Gap(24),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: _isSubmitting ? null : _submit,
                    child: _isSubmitting
                        ? const SizedBox(
                            width: 18,
                            height: 18,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Text('Story Olustur'),
                  ),
                ),
                const Gap(24),
                const Divider(),
                const Gap(16),
                Consumer(
                  builder: (context, ref, child) {
                    final storiesAsync = ref.watch(groupedStoriesProvider);
                    return storiesAsync.when(
                      data: (result) => switch (result) {
                        Success<List<StoryBeachGroup>>(data: final groups) => groups.isEmpty
                            ? const SizedBox.shrink()
                            : Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    'Aktif Story Gruplari',
                                    style: Theme.of(context).textTheme.titleLarge,
                                  ),
                                  const Gap(12),
                                  for (final group in groups)
                                    Padding(
                                      padding: const EdgeInsets.only(bottom: 8),
                                      child: Text(
                                        '${group.beachName} (${group.stories.length})',
                                      ),
                                    ),
                                ],
                              ),
                        FailureResult<List<StoryBeachGroup>>() => const AppErrorWidget(
                            message: 'Story listesi yuklenemedi.',
                          ),
                      },
                      loading: () => const Padding(
                        padding: EdgeInsets.symmetric(vertical: 12),
                        child: AppLoadingIndicator(),
                      ),
                      error: (_, __) => const SizedBox.shrink(),
                    );
                  },
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
