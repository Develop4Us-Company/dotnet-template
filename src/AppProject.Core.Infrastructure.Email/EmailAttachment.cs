using System;

namespace AppProject.Core.Infrastructure.Email;

public class EmailAttachment
{
    required public string Content { get; set; }

    required public string FileName { get; set; }

    required public string Type { get; set; }

    public string Disposition { get; set; } = "attachment";

    public string? ContentId { get; set; }
}
