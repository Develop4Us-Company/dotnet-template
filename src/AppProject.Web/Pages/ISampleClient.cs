using System;
using Refit;

namespace AppProject.Web.Pages;

public interface ISampleClient
{
    [Get("/api/general/sample/GetProtectedData")]
    Task<string> GetProtectedData();

    [Get("/api/general/sample/GetLogSample")]
    Task<string> GetLogSample();
}
