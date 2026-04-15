class RegisterRequestDto {
  const RegisterRequestDto({
    required this.email,
    required this.password,
    this.name,
    this.firstName,
    this.lastName,
    this.phoneNumber,
    this.businessName,
    this.contactName,
  });

  final String email;
  final String password;
  final String? name;
  final String? firstName;
  final String? lastName;
  final String? phoneNumber;
  final String? businessName;
  final String? contactName;

  Map<String, dynamic> toJson() => {
        'email': email,
        'password': password,
        if (name != null && name!.trim().isNotEmpty) 'name': name!.trim(),
        if (firstName != null && firstName!.trim().isNotEmpty)
          'firstName': firstName!.trim(),
        if (lastName != null && lastName!.trim().isNotEmpty)
          'lastName': lastName!.trim(),
        if (phoneNumber != null && phoneNumber!.trim().isNotEmpty)
          'phoneNumber': phoneNumber!.trim(),
        if (businessName != null && businessName!.trim().isNotEmpty)
          'businessName': businessName!.trim(),
        if (contactName != null && contactName!.trim().isNotEmpty)
          'contactName': contactName!.trim(),
      };
}
