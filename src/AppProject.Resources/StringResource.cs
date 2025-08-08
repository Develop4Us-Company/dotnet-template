using System;
using System.Globalization;

namespace AppProject.Resources;

public static class StringResource
{
    public static string GetStringByKey(string key, params object[] args)
    {
        var message = Resource.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ??
            throw new ArgumentException($"Resource with key '{key}' not found.");

        return string.Format(message, args);
    }
}
