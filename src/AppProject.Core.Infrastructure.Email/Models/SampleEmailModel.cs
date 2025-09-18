using System;

namespace AppProject.Core.Infrastructure.Email.Models;

public class SampleEmailModel
{
    public string Name { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.UtcNow;
}
