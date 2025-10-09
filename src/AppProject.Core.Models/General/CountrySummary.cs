using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class CountrySummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
}
