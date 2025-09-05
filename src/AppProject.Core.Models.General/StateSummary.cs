using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class StateSummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string CountryName { get; set; }

    public Guid CountryId { get; set; }
}
