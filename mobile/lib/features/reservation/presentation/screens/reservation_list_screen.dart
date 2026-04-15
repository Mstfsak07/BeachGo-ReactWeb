import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:gap/gap.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/error/result.dart';
import 'package:beachgo/core/theme/app_theme.dart';
import 'package:beachgo/features/reservation/domain/entities/reservation_list_item.dart';
import 'package:beachgo/features/reservation/presentation/providers/reservation_list_provider.dart';
import 'package:beachgo/shared/widgets/shared_widgets.dart';

class ReservationListScreen extends ConsumerWidget {
  const ReservationListScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final reservationsAsync = ref.watch(reservationListProvider);

    return SafeArea(
      child: reservationsAsync.when(
        data: (result) => switch (result) {
          Success<List<ReservationListItem>>(data: final items)
              when items.isNotEmpty =>
            RefreshIndicator(
              onRefresh: () async => ref.invalidate(reservationListProvider),
              child: ListView.separated(
                padding: const EdgeInsets.fromLTRB(20, 24, 20, 24),
                itemCount: items.length,
                separatorBuilder: (_, __) => const Gap(14),
                itemBuilder: (context, index) => _ReservationCard(item: items[index]),
              ),
            ),
          FailureResult<List<ReservationListItem>>(failure: final failure) =>
            AppErrorWidget(
              message: failure.message,
              onRetry: () => ref.invalidate(reservationListProvider),
            ),
          _ => const _ReservationEmpty(),
        },
        loading: () => const Center(child: AppLoadingIndicator()),
        error: (error, _) => AppErrorWidget(
          message: error is Failure ? error.message : 'Rezervasyonlar yuklenemedi.',
          onRetry: () => ref.invalidate(reservationListProvider),
        ),
      ),
    );
  }
}

class _ReservationCard extends StatelessWidget {
  const _ReservationCard({required this.item});

  final ReservationListItem item;

  @override
  Widget build(BuildContext context) {
    final date = DateFormat('d MMMM y', 'tr_TR').format(item.reservationDate);

    return InkWell(
      onTap: () => context.push('/beaches/${item.beachId}'),
      borderRadius: BorderRadius.circular(22),
      child: Ink(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: AppColors.surfaceRaised,
          borderRadius: BorderRadius.circular(22),
          border: Border.all(color: AppColors.panelBorder),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    item.beachName,
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                ),
                _StatusBadge(label: item.status),
              ],
            ),
            const Gap(12),
            Text('$date • ${item.reservationTime}',
                style: Theme.of(context).textTheme.bodyLarge),
            const Gap(6),
            Text(
              'Detaya gitmek icin dokunun.',
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ],
        ),
      ),
    );
  }
}

class _StatusBadge extends StatelessWidget {
  const _StatusBadge({required this.label});

  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: AppColors.primaryLight,
        borderRadius: BorderRadius.circular(999),
      ),
      child: Text(
        label.isEmpty ? 'Beklemede' : label,
        style: Theme.of(context).textTheme.bodySmall?.copyWith(
              color: AppColors.primary,
              fontWeight: FontWeight.w700,
            ),
      ),
    );
  }
}

class _ReservationEmpty extends StatelessWidget {
  const _ReservationEmpty();

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Rezervasyonlarim', style: Theme.of(context).textTheme.headlineMedium),
          const Gap(12),
          Text(
            'Olusturdugunuz rezervasyonlar burada listelenecek.',
            style: Theme.of(context).textTheme.bodyLarge,
          ),
        ],
      ),
    );
  }
}
