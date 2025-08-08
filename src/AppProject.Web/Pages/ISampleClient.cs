using System;
using Refit;

namespace AppProject.Web.Pages;

public interface ISampleClient
{
    [Get("/api/sample/protected")]
    Task<string> GetProtectedMessage();
}
