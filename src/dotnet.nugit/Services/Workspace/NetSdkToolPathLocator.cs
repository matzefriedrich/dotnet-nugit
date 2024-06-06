namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using Abstractions;

    internal sealed class NetSdkToolPathLocator(
        IFileSystem fileSystem) : IMsBuildToolPathLocator
    {
        private readonly IFileSystem fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

        public bool TryLocateMsBuildToolsPath(out string? path)
        {
            path = null;
            List<SdkVersion> sdks = this.GetAdvertisedSdkVersions().ToList();
            SdkVersion? latestSdk = sdks.MaxBy(version => version.Version);
            if (latestSdk == null) return false;

            path = latestSdk.Path;
            return true;
        }

        private IEnumerable<SdkVersion> GetAdvertisedSdkVersions()
        {
            const string dotnetSdkPath = "/usr/lib/dotnet/sdk/";
            if (!this.fileSystem.Directory.Exists(dotnetSdkPath)) yield break;

            string[] directories = this.fileSystem.Directory.GetDirectories(dotnetSdkPath);
            foreach (string directory in directories)
            {
                IDirectoryInfo directoryInfo = this.fileSystem.DirectoryInfo.New(directory);
                string? name = directoryInfo.Name;

                if (!Version.TryParse(name, out Version? sdkVersion))
                    continue;

                yield return new SdkVersion(sdkVersion, directory);
            }
        }
    }
}