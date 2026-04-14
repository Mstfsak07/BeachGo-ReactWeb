class AppUser {
  const AppUser({
    required this.id,
    required this.email,
    required this.role,
    required this.accountType,
    required this.firstName,
    required this.lastName,
    required this.name,
    required this.phone,
  });

  final int? id;
  final String email;
  final String role;
  final String accountType;
  final String firstName;
  final String lastName;
  final String name;
  final String phone;

  String get displayName {
    if (name.isNotEmpty) return name;
    final fullName = [firstName, lastName]
        .where((value) => value.isNotEmpty)
        .join(' ')
        .trim();
    if (fullName.isNotEmpty) return fullName;
    if (email.isNotEmpty) return email.split('@').first;
    return 'Kullanici';
  }

  bool get isBusiness => role == 'Business' || role == 'Admin';
  bool get isAdmin => role == 'Admin';
}
