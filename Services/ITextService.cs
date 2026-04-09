namespace SkyFocus.Services;

internal interface ITextService
{
    string GetString(string key);

    string Format(string key, params object?[] arguments);
}