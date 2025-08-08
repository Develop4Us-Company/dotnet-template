using System;

namespace AppProject.Exceptions;

public class ExceptionDetail
{
    public ExceptionCode ExceptionCode { get; set; }

    public string? AdditionalInfo { get; set; }
}
