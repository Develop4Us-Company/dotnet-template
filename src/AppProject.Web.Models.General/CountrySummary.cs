using System;
using AppProject.Models;

namespace AppProject.Web.Models.General;

public class CountrySummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
}
