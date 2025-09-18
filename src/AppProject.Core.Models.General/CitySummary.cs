using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class CitySummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string StateName { get; set; } = string.Empty;

    public Guid StateId { get; set; }

    public string CountryName { get; set; } = string.Empty;

    public Guid CountryId { get; set; }
}
