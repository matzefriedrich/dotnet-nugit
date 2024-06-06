namespace dotnet.nugit.Services
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Runtime.InteropServices;
    using System.Text;
    using Abstractions;

    public class NuGetConfigurationAccessService(IFileSystem fileSystem) : INuGetConfigurationAccessService
    {
        private readonly IFileSystem fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

        public TextReader GetNuGetConfigReader()
        {
            string nugetConfigFilePath = this.GetNuGetConfigFilePath();
            if (string.IsNullOrWhiteSpace(nugetConfigFilePath) == false && this.fileSystem.File.Exists(nugetConfigFilePath))
            {
                Stream stream = this.fileSystem.File.OpenRead(nugetConfigFilePath);
                return new StreamReader(stream, Encoding.UTF8);
            }

            return TextReader.Null;
        }

        public TextWriter GetNuGetConfigWriter()
        {
            string nugetConfigFilePath = this.GetNuGetConfigFilePath();
            if (string.IsNullOrWhiteSpace(nugetConfigFilePath) == false && this.fileSystem.File.Exists(nugetConfigFilePath))
            {
                Stream stream = this.fileSystem.File.Open(nugetConfigFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
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
                return this.fileSystem.Path.Combine(appDataPath, "NuGet", "NuGet.Config");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return this.fileSystem.Path.Combine(homePath, ".nuget", "NuGet", "NuGet.Config");
            }

            throw new NotSupportedException("The current OS platform is not supported.");
        }
    }
}