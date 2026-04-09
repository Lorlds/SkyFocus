using System.Globalization;
using Microsoft.Windows.ApplicationModel.Resources;

namespace SkyFocus.Services;

internal sealed class TextService : ITextService
{
    private readonly ResourceLoader _resourceLoader = new();

    public string Format(string key, params object?[] arguments)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString(key), arguments);
    }

    public string GetString(string key)
    {
        string value = _resourceLoader.GetString(key);
        return string.IsNullOrWhiteSpace(value) ? key : value;
    }
}