namespace dotnet.nugit.Services
{
    using System.Runtime.InteropServices;
    using Abstractions;

    public class NuGetInfoService : INuGetInfoService
    {
        public string GetNuGetConfigFilePath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // C:\Users\user\AppData\Roaming\NuGet\NuGet.Config
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appDataPath, "NuGet", "NuGet.Config");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(homePath, ".nuget");
            }

            throw new NotSupportedException("The current OS platform is not supported.");
        }
    }
}