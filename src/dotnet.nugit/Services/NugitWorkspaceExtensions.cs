namespace dotnet.nugit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;

    internal static class NugitWorkspaceExtensions
    {
        public static LocalFeedInfo? GetConfiguredLocalFeed(this INugitWorkspace workspace)
        {
            ArgumentNullException.ThrowIfNull(workspace);

            if (workspace.TryReadConfiguration(out NugitConfigurationFile? configurationFile) && configurationFile != null)
                return configurationFile.LocalFeed;

            return null;
        }

        public static IEnumerable<RepositoryReference> GetWorkspaceRepositories(this INugitWorkspace workspace)
        {
            ArgumentNullException.ThrowIfNull(workspace);

            if (workspace.TryReadConfiguration(out NugitConfigurationFile? configurationFile) && configurationFile != null)
                return configurationFile.Repositories.AsReadOnly();

            return Enumerable.Empty<RepositoryReference>();
        }
    }
}