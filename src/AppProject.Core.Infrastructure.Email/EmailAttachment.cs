using System;

namespace AppProject.Core.Infrastructure.Email;

public class EmailAttachment
{
    public string Content { get; set; }

    public string FileName { get; set; }

    public string Type { get; set; }

    public string Disposition { get; set; } = "attachment";

    public string ContentId { get; set; }
}
