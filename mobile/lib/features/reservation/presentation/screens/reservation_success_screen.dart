import 'package:flutter/material.dart';
import 'package:gap/gap.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import 'package:beachgo/core/router/app_router.dart';
import 'package:beachgo/core/theme/app_theme.dart';
import 'package:beachgo/features/reservation/domain/entities/reservation_confirmation.dart';

class ReservationSuccessScreen extends StatelessWidget {
  const ReservationSuccessScreen({
    super.key,
    required this.confirmation,
  });

  final ReservationConfirmation confirmation;

  @override
  Widget build(BuildContext context) {
    final reservationDate = DateFormat(
      'd MMMM y',
      'tr_TR',
    ).format(confirmation.reservationDate.toLocal());

    return Scaffold(
      appBar: AppBar(
        automaticallyImplyLeading: false,
        title: const Text('Rezervasyon Alindi'),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 24, 20, 24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 72,
                height: 72,
                decoration: BoxDecoration(
                  color: AppColors.success.withAlpha(20),
                  borderRadius: BorderRadius.circular(36),
                ),
                child: const Icon(
                  Icons.check_circle_rounded,
                  color: AppColors.success,
                  size: 40,
                ),
              ),
              const Gap(20),
              Text(
                'Rezervasyon isteginiz alindi.',
                style: Theme.of(context).textTheme.headlineMedium,
              ),
              const Gap(8),
              Text(
                'Onaylandiginda detaylarinizi rezervasyonlar ekranindan takip edebilirsiniz.',
                style: Theme.of(context).textTheme.bodyLarge,
              ),
              const Gap(24),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(18),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(color: AppColors.slate200, width: 0.5),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    _SummaryRow(label: 'Plaj', value: confirmation.beachName),
                    const Gap(10),
                    _SummaryRow(label: 'Tarih', value: reservationDate),
                    const Gap(10),
                    _SummaryRow(
                      label: 'Saat',
                      value: confirmation.reservationTime.isNotEmpty
                          ? confirmation.reservationTime
                          : 'Belirtilmedi',
                    ),
                    const Gap(10),
                    _SummaryRow(label: 'Durum', value: confirmation.status),
                    const Gap(10),
                    _SummaryRow(
                      label: 'Rezervasyon No',
                      value: '#${confirmation.id}',
                    ),
                  ],
                ),
              ),
              const Spacer(),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: () {
                    context.goNamed(AppRoute.beachDetail.name, pathParameters: {
                      'id': confirmation.beachId.toString(),
                    });
                  },
                  child: const Text('Plaj Detayina Don'),
                ),
              ),
              const Gap(12),
              SizedBox(
                width: double.infinity,
                child: TextButton(
                  onPressed: () => context.goNamed(AppRoute.reservations.name),
                  child: const Text('Rezervasyonlarimi Ac'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _SummaryRow extends StatelessWidget {
  const _SummaryRow({
    required this.label,
    required this.value,
  });

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: Text(label, style: Theme.of(context).textTheme.bodyMedium),
        ),
        const Gap(12),
        Flexible(
          child: Text(
            value,
            textAlign: TextAlign.right,
            style: Theme.of(context).textTheme.titleMedium,
          ),
        ),
      ],
    );
  }
}
