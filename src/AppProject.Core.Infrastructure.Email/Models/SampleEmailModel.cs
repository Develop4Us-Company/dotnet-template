using System;

namespace AppProject.Core.Infrastructure.Email.Models;

public class SampleEmailModel
{
    public string Name { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
}
