import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:gap/gap.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/core/router/app_router.dart';
import 'package:beachgo/core/theme/app_theme.dart';
import 'package:beachgo/features/beach/domain/entities/beach.dart';
import 'package:beachgo/features/beach/presentation/providers/beach_list_provider.dart';
import 'package:beachgo/features/reservation/presentation/providers/reservation_submit_provider.dart';
import 'package:beachgo/shared/widgets/shared_widgets.dart';

class ReservationScreen extends ConsumerStatefulWidget {
  const ReservationScreen({
    super.key,
    required this.beachId,
    this.initialBeach,
  });

  final int beachId;
  final Beach? initialBeach;

  @override
  ConsumerState<ReservationScreen> createState() => _ReservationScreenState();
}

class _ReservationScreenState extends ConsumerState<ReservationScreen> {
  final _formKey = GlobalKey<FormState>();
  final _guestCountController = TextEditingController(text: '2');
  final _noteController = TextEditingController();

  DateTime? _selectedDate;
  TimeOfDay? _selectedTime;

  bool get _isValidForm {
    final guestCount = int.tryParse(_guestCountController.text.trim()) ?? 0;
    return _selectedDate != null && _selectedTime != null && guestCount > 0;
  }

  @override
  void initState() {
    super.initState();
    _guestCountController.addListener(_handleFieldChanged);
    _noteController.addListener(_handleFieldChanged);
  }

  @override
  void dispose() {
    _guestCountController
      ..removeListener(_handleFieldChanged)
      ..dispose();
    _noteController
      ..removeListener(_handleFieldChanged)
      ..dispose();
    super.dispose();
  }

  void _handleFieldChanged() {
    ref.read(reservationSubmitProvider(widget.beachId).notifier).clearError();
    setState(() {});
  }

  Future<void> _pickDate() async {
    final now = DateTime.now();
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate ?? now.add(const Duration(days: 1)),
      firstDate: DateTime(now.year, now.month, now.day),
      lastDate: now.add(const Duration(days: 180)),
    );

    if (picked == null) {
      return;
    }

    setState(() {
      _selectedDate = DateTime(picked.year, picked.month, picked.day);
    });
    ref.read(reservationSubmitProvider(widget.beachId).notifier).clearError();
  }

  Future<void> _pickTime() async {
    final picked = await showTimePicker(
      context: context,
      initialTime: _selectedTime ?? const TimeOfDay(hour: 10, minute: 0),
    );

    if (picked == null) {
      return;
    }

    setState(() {
      _selectedTime = picked;
    });
    ref.read(reservationSubmitProvider(widget.beachId).notifier).clearError();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate() || !_isValidForm) {
      setState(() {});
      return;
    }

    final selectedDate = _selectedDate;
    final selectedTime = _selectedTime;
    final guestCount = int.tryParse(_guestCountController.text.trim()) ?? 0;

    if (selectedDate == null || selectedTime == null || guestCount <= 0) {
      return;
    }

    final notifier = ref.read(reservationSubmitProvider(widget.beachId).notifier);
    final confirmation = await notifier.submit(
      beachId: widget.beachId,
      reservationDate: selectedDate,
      reservationTime: _formatTimeForApi(selectedTime),
      guestCount: guestCount,
      note: _noteController.text,
    );

    if (!mounted || confirmation == null) {
      return;
    }

    context.pushReplacementNamed(
      AppRoute.reservationSuccess.name,
      extra: confirmation,
    );
  }

  @override
  Widget build(BuildContext context) {
    final submitState = ref.watch(reservationSubmitProvider(widget.beachId));
    final beachAsync = widget.initialBeach == null
        ? ref.watch(beachDetailProvider(widget.beachId))
        : null;

    return Scaffold(
      appBar: AppBar(title: const Text('Rezervasyon Olustur')),
      body: beachAsync == null
          ? _ReservationBody(
              beach: widget.initialBeach!,
              formKey: _formKey,
              guestCountController: _guestCountController,
              noteController: _noteController,
              selectedDate: _selectedDate,
              selectedTime: _selectedTime,
              submitState: submitState,
              isValidForm: _isValidForm,
              onPickDate: _pickDate,
              onPickTime: _pickTime,
              onSubmit: _submit,
            )
          : beachAsync.when(
              data: (result) => switch (result) {
                Success<Beach>(data: final beach) => _ReservationBody(
                    beach: beach,
                    formKey: _formKey,
                    guestCountController: _guestCountController,
                    noteController: _noteController,
                    selectedDate: _selectedDate,
                    selectedTime: _selectedTime,
                    submitState: submitState,
                    isValidForm: _isValidForm,
                    onPickDate: _pickDate,
                    onPickTime: _pickTime,
                    onSubmit: _submit,
                  ),
                FailureResult<Beach>() => const Center(
                    child: AppErrorWidget(
                      message: 'Plaj bilgileri yuklenemedigi icin rezervasyon acilamadi.',
                    ),
                  ),
              },
              loading: () => const FullScreenLoader(),
              error: (_, __) => const Center(
                child: AppErrorWidget(
                  message: 'Rezervasyon bilgileri yuklenemedi.',
                ),
              ),
            ),
    );
  }

  String _formatTimeForApi(TimeOfDay time) {
    final hour = time.hour.toString().padLeft(2, '0');
    final minute = time.minute.toString().padLeft(2, '0');
    return '$hour:$minute';
  }
}

class _ReservationBody extends StatelessWidget {
  const _ReservationBody({
    required this.beach,
    required this.formKey,
    required this.guestCountController,
    required this.noteController,
    required this.selectedDate,
    required this.selectedTime,
    required this.submitState,
    required this.isValidForm,
    required this.onPickDate,
    required this.onPickTime,
    required this.onSubmit,
  });

  final Beach beach;
  final GlobalKey<FormState> formKey;
  final TextEditingController guestCountController;
  final TextEditingController noteController;
  final DateTime? selectedDate;
  final TimeOfDay? selectedTime;
  final ReservationSubmitState submitState;
  final bool isValidForm;
  final VoidCallback onPickDate;
  final VoidCallback onPickTime;
  final VoidCallback onSubmit;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final dateLabel = selectedDate == null
        ? 'Tarih secin'
        : DateFormat('d MMMM y', 'tr_TR').format(selectedDate!.toLocal());
    final timeLabel = selectedTime == null
        ? 'Saat secin'
        : selectedTime!.format(context);

    return SafeArea(
      child: SingleChildScrollView(
        padding: const EdgeInsets.fromLTRB(20, 20, 20, 28),
        child: Form(
          key: formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                  gradient: const LinearGradient(
                    colors: [AppColors.surfaceAlt, AppColors.primaryDark],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  borderRadius: BorderRadius.circular(28),
                  border: Border.all(color: AppColors.panelBorder),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 8,
                      ),
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.16),
                        borderRadius: BorderRadius.circular(999),
                      ),
                      child: Text(
                        'Rezervasyon talebi',
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: Colors.white,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ),
                    const Gap(16),
                    Text(
                      beach.name,
                      style: theme.textTheme.headlineMedium?.copyWith(
                        color: Colors.white,
                      ),
                    ),
                    if (beach.address.isNotEmpty) ...[
                      const Gap(8),
                      Text(
                        beach.address,
                        style: theme.textTheme.bodyLarge?.copyWith(
                          color: Colors.white.withValues(alpha: 0.86),
                        ),
                      ),
                    ],
                    const Gap(16),
                    Row(
                      children: [
                        Expanded(
                          child: _HeroStat(
                            icon: Icons.schedule_outlined,
                            label: '${beach.openTime} - ${beach.closeTime}',
                          ),
                        ),
                        const Gap(10),
                        Expanded(
                          child: _HeroStat(
                            icon: Icons.people_alt_outlined,
                            label:
                                '%${beach.occupancyPercent.toStringAsFixed(0)} dolu',
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              const Gap(20),
              Text('Rezervasyon detaylari', style: theme.textTheme.titleLarge),
              const Gap(8),
              Text(
                'Tarih, saat ve kisi sayisini girin. Talebiniz gonderilmeden once tum bilgiler sizde kalir.',
                style: theme.textTheme.bodyMedium,
              ),
              const Gap(16),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(18),
                decoration: BoxDecoration(
                  color: AppColors.surfaceRaised,
                  borderRadius: BorderRadius.circular(22),
                  border: Border.all(color: AppColors.panelBorder),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Ziyaret planiniz', style: theme.textTheme.titleMedium),
                    const Gap(14),
                    _PickerField(
                      label: 'Tarih',
                      value: dateLabel,
                      icon: Icons.calendar_today_outlined,
                      onTap: onPickDate,
                      hasError: selectedDate == null,
                    ),
                    const Gap(12),
                    _PickerField(
                      label: 'Saat',
                      value: timeLabel,
                      icon: Icons.access_time_rounded,
                      onTap: onPickTime,
                      hasError: selectedTime == null,
                    ),
                    const Gap(12),
                    TextFormField(
                      controller: guestCountController,
                      keyboardType: TextInputType.number,
                      inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                      decoration: const InputDecoration(
                        labelText: 'Kisi sayisi',
                        hintText: 'Ornek: 2',
                        prefixIcon: Icon(Icons.groups_rounded),
                      ),
                      validator: (value) {
                        final guestCount = int.tryParse(value?.trim() ?? '');
                        if (guestCount == null || guestCount <= 0) {
                          return 'Gecerli bir kisi sayisi girin.';
                        }
                        return null;
                      },
                    ),
                    const Gap(12),
                    TextFormField(
                      controller: noteController,
                      minLines: 3,
                      maxLines: 4,
                      decoration: const InputDecoration(
                        labelText: 'Not',
                        hintText: 'Opsiyonel bir not ekleyin',
                        alignLabelWithHint: true,
                        prefixIcon: Icon(Icons.edit_note_rounded),
                      ),
                    ),
                  ],
                ),
              ),
              const Gap(16),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(18),
                decoration: BoxDecoration(
                  color: AppColors.surfaceRaised,
                  borderRadius: BorderRadius.circular(22),
                  border: Border.all(
                    color: AppColors.panelBorder,
                    width: 1,
                  ),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Container(
                          width: 40,
                          height: 40,
                          decoration: BoxDecoration(
                            color: AppColors.primaryLight,
                            borderRadius: BorderRadius.circular(14),
                            border: Border.all(color: AppColors.panelBorder),
                          ),
                          child: const Icon(
                            Icons.verified_user_outlined,
                            color: AppColors.primary,
                          ),
                        ),
                        const Gap(12),
                        Expanded(
                          child: Text(
                            'Rezervasyon ozeti',
                            style: theme.textTheme.titleMedium,
                          ),
                        ),
                      ],
                    ),
                    const Gap(8),
                    Text('Plaj: ${beach.name}', style: theme.textTheme.bodyLarge),
                    const Gap(4),
                    Text(
                      'Giris ucreti ve diger tutarlar sunucuda dogrulanir.',
                      style: theme.textTheme.bodyMedium,
                    ),
                  ],
                ),
              ),
              if (submitState.errorMessage != null) ...[
                const Gap(16),
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: const Color(0x33FF6B6B),
                    borderRadius: BorderRadius.circular(18),
                    border: Border.all(
                      color: AppColors.error.withValues(alpha: 0.35),
                    ),
                  ),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Icon(
                        Icons.error_outline_rounded,
                        color: AppColors.error,
                      ),
                      const Gap(10),
                      Expanded(
                        child: Text(
                          submitState.errorMessage!,
                          style: const TextStyle(
                            color: AppColors.error,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
              if (!isValidForm) ...[
                const Gap(14),
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    color: AppColors.surfaceRaised,
                    borderRadius: BorderRadius.circular(18),
                    border: Border.all(color: AppColors.panelBorder),
                  ),
                  child: Text(
                    'Devam etmek icin tarih, saat ve kisi sayisini tamamlayin.',
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: AppColors.slate600,
                    ),
                  ),
                ),
              ],
              const Gap(24),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  onPressed:
                      submitState.isSubmitting || !isValidForm ? null : onSubmit,
                  icon: submitState.isSubmitting
                      ? const SizedBox(
                          width: 18,
                          height: 18,
                          child: CircularProgressIndicator(
                            strokeWidth: 2.2,
                            valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
                          ),
                        )
                      : const Icon(Icons.event_available_rounded),
                  label: Text(
                    submitState.isSubmitting
                        ? 'Gonderiliyor...'
                        : 'Rezervasyonu Gonder',
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _PickerField extends StatelessWidget {
  const _PickerField({
    required this.label,
    required this.value,
    required this.icon,
    required this.onTap,
    required this.hasError,
  });

  final String label;
  final String value;
  final IconData icon;
  final VoidCallback onTap;
  final bool hasError;

  @override
  Widget build(BuildContext context) {
    final borderColor = hasError ? AppColors.error : AppColors.slate200;
    final valueColor = hasError ? AppColors.slate400 : AppColors.slate900;

    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(18),
      child: Ink(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        decoration: BoxDecoration(
          color: AppColors.surfaceAlt,
          borderRadius: BorderRadius.circular(18),
          border: Border.all(color: borderColor),
        ),
        child: Row(
          children: [
            Icon(icon, color: AppColors.slate500),
            const Gap(12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(label, style: Theme.of(context).textTheme.bodySmall),
                  const Gap(2),
                  Text(
                    value,
                    style: TextStyle(
                      color: valueColor,
                      fontSize: 15,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _HeroStat extends StatelessWidget {
  const _HeroStat({
    required this.icon,
    required this.label,
  });

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 12),
      decoration: BoxDecoration(
        color: Colors.white.withValues(alpha: 0.14),
        borderRadius: BorderRadius.circular(18),
      ),
      child: Row(
        children: [
          Icon(icon, color: Colors.white, size: 18),
          const Gap(8),
          Expanded(
            child: Text(
              label,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.w700,
                  ),
            ),
          ),
        ],
      ),
    );
  }
}
