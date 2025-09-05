using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class CitySummary : ISummary
{
    public Guid Id { get; set; }

    required public string Name { get; set; }

    required public string StateName { get; set; }

    public Guid StateId { get; set; }

    required public string CountryName { get; set; }

    public Guid CountryId { get; set; }
}
