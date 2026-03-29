namespace BeachRehberi.Domain.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string message, string code = "DOMAIN_ERROR")
        : base(message)
    {
        Code = code;
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} bulunamadı. (Id: {key})", "NOT_FOUND") { }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Bu işlem için yetkiniz yok.")
        : base(message, "UNAUTHORIZED") { }
}

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "Bu kaynağa erişim yasak.")
        : base(message, "FORBIDDEN") { }
}

public class ValidationException : DomainException
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors)
        : base("Doğrulama hataları var.", "VALIDATION_ERROR")
    {
        Errors = errors;
    }
}

public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message)
        : base(message, "BUSINESS_RULE_VIOLATION") { }
}

public class TenantLimitExceededException : DomainException
{
    public TenantLimitExceededException(string limitName)
        : base($"Plan limitiniz aşıldı: {limitName}. Lütfen planınızı yükseltin.", "LIMIT_EXCEEDED") { }
}
