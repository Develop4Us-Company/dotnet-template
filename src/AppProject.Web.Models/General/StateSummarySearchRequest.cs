using System;
using AppProject.Models;

namespace AppProject.Web.Models.General;

public class StateSummarySearchRequest : SearchRequest
{
    public Guid? CountryId { get; set; }
}
