using System;
using AppProject.Models;

namespace AppProject.Web.Models.General;

public class CitySummarySearchRequest : SearchRequest
{
    public Guid? StateId { get; set; }
}
