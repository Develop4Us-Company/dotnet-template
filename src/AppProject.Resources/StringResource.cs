using System;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;

namespace AppProject.Resources;

public static class StringResource
{
    public static string GetStringByKey(string key, params object?[] args)
    {
        var message = Resource.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? string.Empty;

        return string.Format(message, args);
    }
}
