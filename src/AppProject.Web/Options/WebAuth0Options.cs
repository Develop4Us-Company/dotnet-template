using System;

namespace AppProject.Web.Options;

public class WebAuth0Options
{
    public string? Authority { get; set; }

    public string? ClientId { get; set; }

    public string? Audience { get; set; }
}
