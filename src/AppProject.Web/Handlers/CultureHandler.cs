using System;
using System.Globalization;

namespace AppProject.Web.Handlers;

public class CultureHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Contains("Accept-Language"))
        {
            request.Headers.Remove("Accept-Language");
        }

        request.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

        return base.SendAsync(request, cancellationToken);
    }
}
