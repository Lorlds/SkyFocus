using System.IO;
using Windows.Storage;

namespace SkyFocus.Services;

internal sealed class PackagedStoragePathProvider : IStoragePathProvider
{
    public string GetLocalDataPath()
    {
        string basePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "SkyFocus");
        Directory.CreateDirectory(basePath);
        return basePath;
    }
}