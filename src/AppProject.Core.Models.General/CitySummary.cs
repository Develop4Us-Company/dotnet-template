using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class CitySummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string StateName { get; set; } = default!;

    public Guid StateId { get; set; }

    public string CountryName { get; set; } = default!;

    public Guid CountryId { get; set; }
}
