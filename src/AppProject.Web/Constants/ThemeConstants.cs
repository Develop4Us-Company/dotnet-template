using System;

namespace AppProject.Web.Constants;

public class ThemeConstants
{
    public const string DefaultLightTheme = "standard";

    public const string DefaultDarkTheme = "standard-dark";

    public const string ThemeLocalStorageKey = $"{AppProjectConstants.LocalStorageKeyPrefix}Theme";
}
