using System;
using System.Collections.Generic;

namespace BeachRehberi.API.Exceptions
{
    public class DomainException : Exception
    {
        public int StatusCode { get; }
        public List<string> Errors { get; }

        public DomainException(string message, int statusCode = 400, List<string>? errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors ?? new List<string>();
        }
    }

    public class NotFoundException : DomainException
    {
        public NotFoundException(string message = "Kaynak bulunamadı")
            : base(message, 404) { }
    }

    public class ValidationException : DomainException
    {
        public ValidationException(string message = "Doğrulama hatası", List<string>? errors = null)
            : base(message, 422, errors) { }
    }

    public class UnauthorizedException : DomainException
    {
        public UnauthorizedException(string message = "Yetkisiz erişim")
            : base(message, 401) { }
    }
}
