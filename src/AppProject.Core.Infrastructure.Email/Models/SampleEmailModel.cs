using System;

namespace AppProject.Core.Infrastructure.Email.Models;

public class SampleEmailModel
{
    required public string Name { get; set; }

    required public DateTime Date { get; set; } = DateTime.UtcNow;
}
