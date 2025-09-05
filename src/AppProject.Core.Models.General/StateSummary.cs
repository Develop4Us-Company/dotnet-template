using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class StateSummary : ISummary
{
    public Guid Id { get; set; }

    required public string Name { get; set; }

    required public string CountryName { get; set; }

    public Guid CountryId { get; set; }
}
