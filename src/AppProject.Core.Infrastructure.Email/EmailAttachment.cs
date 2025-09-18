using System;

namespace AppProject.Core.Infrastructure.Email;

public class EmailAttachment
{
    public string Content { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Disposition { get; set; } = "attachment";

    public string? ContentId { get; set; }
}
