import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:gap/gap.dart';
import 'package:go_router/go_router.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/router/app_router.dart';
import 'package:beachgo/core/theme/app_theme.dart';
import 'package:beachgo/features/auth/presentation/providers/auth_provider.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({
    super.key,
    this.isBusinessMode = false,
  });

  final bool isBusinessMode;

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();

  bool _obscurePassword = true;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    await ref.read(authProvider.notifier).login(
          _emailController.text.trim(),
          _passwordController.text,
        );
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authProvider);
    final isSubmitting = authState.isLoading;
    final errorMessage = _errorMessage(authState.error);
    final isBusinessMode = widget.isBusinessMode;
    final title = isBusinessMode ? 'Isletme hesabinizla giris yapin' : 'Hesabiniza giris yapin';
    final subtitle = isBusinessMode
        ? 'Panelinize ulasin, rezervasyonlari yonetin ve plaj bilgilerinizi guncelleyin.'
        : 'Kayitli kullanicilar rezervasyon ve favori islemlerine buradan devam eder.';
    final submitLabel = isBusinessMode ? 'Isletme Girisi Yap' : 'Giris Yap';

    return Scaffold(
      body: SafeArea(
        child: Stack(
          children: [
            Positioned(
              top: -40,
              right: -40,
              child: Container(
                width: 180,
                height: 180,
                decoration: BoxDecoration(
                  color: AppColors.primary.withValues(alpha: 0.08),
                  borderRadius: BorderRadius.circular(90),
                ),
              ),
            ),
            Positioned(
              top: 90,
              left: -30,
              child: Container(
                width: 100,
                height: 100,
                decoration: BoxDecoration(
                  color: AppColors.sea.withValues(alpha: 0.08),
                  borderRadius: BorderRadius.circular(50),
                ),
              ),
            ),
            Center(
              child: SingleChildScrollView(
                padding: const EdgeInsets.fromLTRB(24, 32, 24, 24),
                child: ConstrainedBox(
                  constraints: const BoxConstraints(maxWidth: 420),
                  child: Form(
                    key: _formKey,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Container(
                          width: 76,
                          height: 76,
                          decoration: BoxDecoration(
                            gradient: const LinearGradient(
                              colors: [AppColors.primaryDark, AppColors.primary],
                              begin: Alignment.topLeft,
                              end: Alignment.bottomRight,
                            ),
                            borderRadius: BorderRadius.circular(24),
                            boxShadow: const [
                              BoxShadow(
                                color: Color(0x221570EF),
                                blurRadius: 24,
                                offset: Offset(0, 14),
                              ),
                            ],
                          ),
                          child: const Icon(
                            Icons.beach_access_rounded,
                            color: Colors.white,
                            size: 34,
                          ),
                        ),
                        const Gap(24),
                        Text('BeachGo', style: Theme.of(context).textTheme.headlineLarge),
                        const Gap(10),
                        Text(
                          isBusinessMode
                              ? 'Isletmenizi mobilde de yonetin. Rezervasyon akisini takip edin ve plaj sayfanizi canli tutun.'
                              : 'Sahile daha hizli ulasin. Favori plajlari kesfedin, storyleri izleyin ve hazir oldugunuzda rezervasyonunuzu tamamlayin.',
                          style: Theme.of(context).textTheme.bodyLarge,
                        ),
                        const Gap(28),
                        Container(
                          width: double.infinity,
                          padding: const EdgeInsets.all(22),
                          decoration: BoxDecoration(
                            color: AppColors.surfaceRaised,
                            borderRadius: BorderRadius.circular(28),
                            border: Border.all(
                              color: AppColors.panelBorder,
                              width: 0.8,
                            ),
                          ),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                title,
                                style: Theme.of(context).textTheme.titleLarge,
                              ),
                              const Gap(6),
                              Text(
                                subtitle,
                                style: Theme.of(context).textTheme.bodyMedium,
                              ),
                              const Gap(18),
                              TextFormField(
                                controller: _emailController,
                                keyboardType: TextInputType.emailAddress,
                                autofillHints: const [AutofillHints.username],
                                textInputAction: TextInputAction.next,
                                decoration: const InputDecoration(
                                  labelText: 'E-posta',
                                  hintText: 'ornek@email.com',
                                  prefixIcon: Icon(Icons.mail_outline_rounded),
                                ),
                                validator: (value) {
                                  final email = value?.trim() ?? '';
                                  if (email.isEmpty) {
                                    return 'E-posta gereklidir.';
                                  }
                                  if (!email.contains('@')) {
                                    return 'Gecerli bir e-posta girin.';
                                  }
                                  return null;
                                },
                              ),
                              const Gap(14),
                              TextFormField(
                                controller: _passwordController,
                                obscureText: _obscurePassword,
                                autofillHints: const [AutofillHints.password],
                                textInputAction: TextInputAction.done,
                                onFieldSubmitted: (_) => _submit(),
                                decoration: InputDecoration(
                                  labelText: 'Sifre',
                                  prefixIcon:
                                      const Icon(Icons.lock_outline_rounded),
                                  suffixIcon: IconButton(
                                    onPressed: () {
                                      setState(() {
                                        _obscurePassword = !_obscurePassword;
                                      });
                                    },
                                    icon: Icon(
                                      _obscurePassword
                                          ? Icons.visibility_off_outlined
                                          : Icons.visibility_outlined,
                                    ),
                                  ),
                                ),
                                validator: (value) {
                                  if ((value ?? '').isEmpty) {
                                    return 'Sifre gereklidir.';
                                  }
                                  return null;
                                },
                              ),
                            ],
                          ),
                        ),
                        if (errorMessage != null) ...[
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
                                    errorMessage,
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
                        const Gap(20),
                        SizedBox(
                          width: double.infinity,
                          child: ElevatedButton(
                            onPressed: isSubmitting ? null : _submit,
                            child: isSubmitting
                                ? const SizedBox(
                                    width: 20,
                                    height: 20,
                                    child: CircularProgressIndicator(
                                      strokeWidth: 2.2,
                                      valueColor:
                                          AlwaysStoppedAnimation<Color>(Colors.white),
                                    ),
                                  )
                                : Text(submitLabel),
                          ),
                        ),
                        const Gap(12),
                        if (isBusinessMode) ...[
                          SizedBox(
                            width: double.infinity,
                            child: OutlinedButton(
                              onPressed: () =>
                                  context.goNamed(AppRoute.businessRegister.name),
                              child: const Text('Isletme Kaydi'),
                            ),
                          ),
                          const Gap(8),
                          SizedBox(
                            width: double.infinity,
                            child: TextButton(
                              onPressed: () => context.goNamed(AppRoute.login.name),
                              child: const Text('Kullanici Girisine Don'),
                            ),
                          ),
                        ] else ...[
                          SizedBox(
                            width: double.infinity,
                            child: OutlinedButton(
                              onPressed: () => context.goNamed(AppRoute.register.name),
                              child: const Text('Kayit Ol'),
                            ),
                          ),
                          const Gap(8),
                          SizedBox(
                            width: double.infinity,
                            child: OutlinedButton(
                              onPressed: () =>
                                  context.goNamed(AppRoute.businessLogin.name),
                              child: const Text('Isletme Girisi'),
                            ),
                          ),
                          const Gap(8),
                        ],
                        SizedBox(
                          width: double.infinity,
                          child: TextButton(
                            onPressed: () => context.goNamed(
                              AppRoute.appShell.name,
                              pathParameters: {'tab': 'home'},
                            ),
                            child: const Text('Misafir Olarak Devam Et'),
                          ),
                        ),
                        const Gap(16),
                        Text(
                          isBusinessMode
                              ? 'Yeni bir isletme hesabi acmak icin Isletme Kaydi aksiyonunu kullanabilirsiniz.'
                              : 'Yeni bir hesap olusturmak icin Kayit Ol aksiyonunu kullanabilir, isletme hesabiniz varsa Isletme Girisi yolunu kullanabilirsiniz.',
                          style: Theme.of(context).textTheme.bodySmall,
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  String? _errorMessage(Object? error) {
    if (error is Failure && error.message.trim().isNotEmpty) {
      return error.message;
    }
    if (error != null) {
      return 'Giris yapilamadi. Bilgilerinizi kontrol edip tekrar deneyin.';
    }
    return null;
  }
}
