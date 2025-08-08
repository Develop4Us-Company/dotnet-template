using System;

namespace AppProject.Exceptions;

public class AppException : Exception
{
    public AppException(
            ExceptionCode exceptionCode = ExceptionCode.Generic, string? additionalInfo = null, Exception? innerException = null)
            : base(innerException?.Message, innerException)
    {
        this.ExceptionCode = exceptionCode;
        this.AdditionalInfo = additionalInfo;
    }

    public ExceptionCode ExceptionCode { get; }

    public string? AdditionalInfo { get; }
}
