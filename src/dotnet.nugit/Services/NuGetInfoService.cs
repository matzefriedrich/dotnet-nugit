namespace dotnet.nugit.Services
{
    using System.Runtime.InteropServices;
    using System.Text;
    using Abstractions;

    public class NuGetInfoService : INuGetInfoService
    {
        public TextReader GetNuGetConfigReader()
        {
            string nugetConfigFilePath = this.GetNuGetConfigFilePath();
            if (string.IsNullOrWhiteSpace(nugetConfigFilePath) == false && File.Exists(nugetConfigFilePath))
            {
                Stream stream = File.OpenRead(nugetConfigFilePath);
                return new StreamReader(stream, Encoding.UTF8);
            }

            return TextReader.Null;
        }

        public TextWriter GetNuGetConfigWriter()
        {
            string nugetConfigFilePath = this.GetNuGetConfigFilePath();
            if (string.IsNullOrWhiteSpace(nugetConfigFilePath) == false && File.Exists(nugetConfigFilePath))
            {
                Stream stream = File.Open(nugetConfigFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                stream.SetLength(0);
                return new StreamWriter(stream, Encoding.UTF8, 4096);
            }

            return TextWriter.Null;
        }

        private string GetNuGetConfigFilePath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appDataPath, "NuGet", "NuGet.Config");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(homePath, ".nuget", "NuGet", "NuGet.Config");
            }

            throw new NotSupportedException("The current OS platform is not supported.");
        }
    }
}