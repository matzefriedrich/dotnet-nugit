﻿namespace dotnet.nugit.Services.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using nugit.Abstractions;

    public sealed class FindAndBuildProjectsTask(
        INugitWorkspace workspace,
        IDotNetUtility dotNetUtility,
        IFindFilesService finder,
        ILogger<FindAndBuildProjectsTask> logger)
    {
        private readonly IDotNetUtility dotNetUtility = dotNetUtility ?? throw new ArgumentNullException(nameof(dotNetUtility));
        private readonly IFindFilesService finder = finder ?? throw new ArgumentNullException(nameof(finder));
        private readonly ILogger<FindAndBuildProjectsTask> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly INugitWorkspace workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

        public async Task FindAndBuildPackagesAsync(RepositoryReference qualifiedRepositoryReference, LocalFeedInfo feed, CancellationToken cancellationToken)
        {
            string? commitSha = qualifiedRepositoryReference.Hash?[..7];
            string versionSuffix = qualifiedRepositoryReference.Tag ?? $"ref-{commitSha}";

            string localRepositoryPath = feed.ProjectDirectoryPathFor(qualifiedRepositoryReference.AsRepositoryUri());
            IAsyncEnumerable<string> projectFileFinder = this.CreateDotNetProjectFileFinder(localRepositoryPath, cancellationToken);
            List<string> projectFiles = await projectFileFinder.ToListAsync(cancellationToken);
            foreach (string file in projectFiles)
            {
                TimeSpan timeout = TimeSpan.FromSeconds(30);

                this.logger.LogInformation("Building package for project: {ProjectFile}@{ReferenceName}", file, versionSuffix);
                await this.dotNetUtility.BuildAsync(file, timeout, cancellationToken);

                string packageTargetFolderPath = feed.PackagesRootPath();
                var packOptions = new PackOptions { VersionSuffix = versionSuffix };
                bool success = await this.dotNetUtility.TryPackAsync(file, packageTargetFolderPath, packOptions, timeout, cancellationToken);
                if (success == false) this.logger.LogWarning("Failed to create NuGet package for project: {ProjectFile}@{ReferenceName}", file, versionSuffix);
                if (this.workspace.TryReadConfiguration(out NugitConfigurationFile? config) && config is { CopyLocal: true })
                {
                    packageTargetFolderPath = Path.Combine(Environment.CurrentDirectory, "nupkg");
                    await this.dotNetUtility.TryPackAsync(file, packageTargetFolderPath, packOptions, timeout, cancellationToken);
                }
            }
        }

        private IAsyncEnumerable<string> CreateDotNetProjectFileFinder(string localRepositoryPath, CancellationToken cancellationToken)
        {
            const string csproj = ".csproj";
            const string vbproj = ".vbproj";

            return this.finder.FindAsync(localRepositoryPath, "*.*", async entry =>
            {
                if (entry.IsDirectory) return await Task.FromResult(true);
                string extension = Path.GetExtension(entry.Path);
                return extension switch
                {
                    csproj => true,
                    vbproj => true,
                    _ => false
                };
            }, cancellationToken);
        }
    }
}