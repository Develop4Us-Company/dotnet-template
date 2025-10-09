using System;
using AppProject.Models;

namespace AppProject.Web.Models.General;

public class StateSearchRequest : SearchRequest
{
    public Guid? CountryId { get; set; }
}
