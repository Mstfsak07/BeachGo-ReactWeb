import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:gap/gap.dart';
import 'package:go_router/go_router.dart';

import 'package:beachgo/core/error/failures.dart';
import 'package:beachgo/core/router/app_router.dart';
import 'package:beachgo/core/theme/app_theme.dart';
import 'package:beachgo/features/auth/presentation/providers/auth_provider.dart';

class RegisterScreen extends ConsumerStatefulWidget {
  const RegisterScreen({
    super.key,
    this.isBusiness = false,
  });

  final bool isBusiness;

  @override
  ConsumerState<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends ConsumerState<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _businessNameController = TextEditingController();
  final _contactNameController = TextEditingController();
  final _phoneController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();

  bool _obscurePassword = true;
  bool _obscureConfirmPassword = true;

  @override
  void dispose() {
    _nameController.dispose();
    _businessNameController.dispose();
    _contactNameController.dispose();
    _phoneController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    if (widget.isBusiness) {
      await ref.read(authProvider.notifier).businessRegister(
            businessName: _businessNameController.text.trim(),
            contactName: _contactNameController.text.trim(),
            email: _emailController.text.trim(),
            password: _passwordController.text,
            phoneNumber: _phoneController.text.trim(),
          );
    } else {
      await ref.read(authProvider.notifier).register(
            name: _nameController.text.trim(),
            email: _emailController.text.trim(),
            password: _passwordController.text,
          );
    }

    if (!mounted) {
      return;
    }

    final authState = ref.read(authProvider);
    if (!authState.hasError) {
      ScaffoldMessenger.of(context)
        ..hideCurrentSnackBar()
        ..showSnackBar(
          SnackBar(
            content: Text(
              widget.isBusiness
                  ? 'Isletme kaydi tamamlandi. Giris yaparak devam edebilirsiniz.'
                  : 'Kayit tamamlandi. Giris yaparak devam edebilirsiniz.',
            ),
          ),
        );
      context.goNamed(
        widget.isBusiness ? AppRoute.businessLogin.name : AppRoute.login.name,
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authProvider);
    final isSubmitting = authState.isLoading;
    final errorMessage = _errorMessage(authState.error);
    final isBusiness = widget.isBusiness;

    return Scaffold(
      appBar: AppBar(),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(24, 8, 24, 24),
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 460),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      isBusiness ? 'Isletme Kaydi' : 'Hesap Olustur',
                      style: Theme.of(context).textTheme.headlineLarge,
                    ),
                    const Gap(10),
                    Text(
                      isBusiness
                          ? 'Plajinizi yonetmek, rezervasyonlari takip etmek ve panel erisimi almak icin isletme hesabinizi olusturun.'
                          : 'Favoriler, rezervasyonlar ve kisisel deneyim icin hizlica kaydolun.',
                      style: Theme.of(context).textTheme.bodyLarge,
                    ),
                    const Gap(24),
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.all(22),
                      decoration: BoxDecoration(
                        color: AppColors.surfaceRaised,
                        borderRadius: BorderRadius.circular(28),
                        border: Border.all(color: AppColors.panelBorder),
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          if (isBusiness) ...[
                            TextFormField(
                              controller: _businessNameController,
                              decoration: const InputDecoration(
                                labelText: 'Isletme adi',
                                prefixIcon: Icon(Icons.storefront_outlined),
                              ),
                              validator: (value) {
                                if ((value ?? '').trim().isEmpty) {
                                  return 'Isletme adi gereklidir.';
                                }
                                return null;
                              },
                            ),
                            const Gap(14),
                            TextFormField(
                              controller: _contactNameController,
                              decoration: const InputDecoration(
                                labelText: 'Yetkili kisi',
                                prefixIcon: Icon(Icons.badge_outlined),
                              ),
                              validator: (value) {
                                if ((value ?? '').trim().isEmpty) {
                                  return 'Yetkili kisi gereklidir.';
                                }
                                return null;
                              },
                            ),
                            const Gap(14),
                            TextFormField(
                              controller: _phoneController,
                              keyboardType: TextInputType.phone,
                              decoration: const InputDecoration(
                                labelText: 'Telefon',
                                prefixIcon: Icon(Icons.call_outlined),
                              ),
                            ),
                          ] else ...[
                            TextFormField(
                              controller: _nameController,
                              decoration: const InputDecoration(
                                labelText: 'Ad soyad',
                                prefixIcon: Icon(Icons.person_outline_rounded),
                              ),
                              validator: (value) {
                                if ((value ?? '').trim().isEmpty) {
                                  return 'Ad soyad gereklidir.';
                                }
                                return null;
                              },
                            ),
                          ],
                          const Gap(14),
                          TextFormField(
                            controller: _emailController,
                            keyboardType: TextInputType.emailAddress,
                            decoration: const InputDecoration(
                              labelText: 'E-posta',
                              prefixIcon: Icon(Icons.mail_outline_rounded),
                            ),
                            validator: (value) {
                              final email = (value ?? '').trim();
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
                            decoration: InputDecoration(
                              labelText: 'Sifre',
                              prefixIcon: const Icon(Icons.lock_outline_rounded),
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
                              if ((value ?? '').length < 6) {
                                return 'Sifre en az 6 karakter olmali.';
                              }
                              return null;
                            },
                          ),
                          const Gap(14),
                          TextFormField(
                            controller: _confirmPasswordController,
                            obscureText: _obscureConfirmPassword,
                            decoration: InputDecoration(
                              labelText: 'Sifre tekrari',
                              prefixIcon: const Icon(Icons.lock_reset_rounded),
                              suffixIcon: IconButton(
                                onPressed: () {
                                  setState(() {
                                    _obscureConfirmPassword =
                                        !_obscureConfirmPassword;
                                  });
                                },
                                icon: Icon(
                                  _obscureConfirmPassword
                                      ? Icons.visibility_off_outlined
                                      : Icons.visibility_outlined,
                                ),
                              ),
                            ),
                            validator: (value) {
                              if (value != _passwordController.text) {
                                return 'Sifreler eslesmiyor.';
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
                        child: Text(
                          errorMessage,
                          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                color: AppColors.error,
                                fontWeight: FontWeight.w700,
                              ),
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
                                width: 18,
                                height: 18,
                                child: CircularProgressIndicator(strokeWidth: 2.2),
                              )
                            : Text(
                                isBusiness
                                    ? 'Isletme Kaydini Tamamla'
                                    : 'Kaydi Tamamla',
                              ),
                      ),
                    ),
                    const Gap(10),
                    SizedBox(
                      width: double.infinity,
                      child: TextButton(
                        onPressed: () => context.goNamed(
                          isBusiness ? AppRoute.businessLogin.name : AppRoute.login.name,
                        ),
                        child: const Text('Zaten hesabim var'),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  String? _errorMessage(Object? error) {
    if (error is Failure && error.message.trim().isNotEmpty) {
      return error.message;
    }
    if (error != null) {
      return 'Kayit su anda tamamlanamadi. Bilgileri kontrol edip tekrar deneyin.';
    }
    return null;
  }
}
