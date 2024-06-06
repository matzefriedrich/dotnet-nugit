namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.Linq;
    using Abstractions;
    using Microsoft.Build.Locator;
    using Microsoft.Extensions.Logging;

    internal sealed class VisualStudioToolPathLocator(ILogger<VisualStudioToolPathLocator> logger) : IMsBuildToolPathLocator
    {
        private readonly ILogger<VisualStudioToolPathLocator> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public bool TryLocateMsBuildToolsPath(out string? path)
        {
            path = null;
            if (OperatingSystem.IsWindows() == false)
                return false;

            this.logger.LogInformation("Querying available Visual Studio instances.");
            VisualStudioInstance[] visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();

            VisualStudioInstance? first = visualStudioInstances.FirstOrDefault();
            if (first == null) throw new InvalidOperationException("Failed to locate Visual Studio.");

            MSBuildLocator.RegisterInstance(first);
            this.logger.LogInformation("Registered {VisualStudioInstance} {VisualStudioVersion} with the current MSBuild locator.", first.Name, first.Version);

            path = first.MSBuildPath;
            return true;
        }
    }
}