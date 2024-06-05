namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Host.Mef;
    using Microsoft.CodeAnalysis.MSBuild;
    using Microsoft.Extensions.Logging;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using Resources;

    public class ProjectWorkspaceManager : IProjectWorkspaceManager
    {
        private readonly ILogger<ProjectWorkspaceManager> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly IEnumerable<IMsBuildToolPathLocator> msBuildLocators;
        private readonly ProjectWorkspaceLogger msBuildLogger;

        public ProjectWorkspaceManager(
            IEnumerable<IMsBuildToolPathLocator> msBuildLocators,
            ILoggerFactory loggerFactory,
            ILogger<ProjectWorkspaceManager> logger)
        {
            this.msBuildLocators = msBuildLocators ?? throw new ArgumentNullException(nameof(msBuildLocators));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ILogger<ProjectWorkspaceLogger> workspaceLogger = loggerFactory.CreateLogger<ProjectWorkspaceLogger>();
            this.msBuildLogger = new ProjectWorkspaceLogger(workspaceLogger);
        }

        public string? LocateMsBuildToolsPath()
        {
            foreach (IMsBuildToolPathLocator toolPathLocator in this.msBuildLocators)
            {
                if (toolPathLocator.TryLocateMsBuildToolsPath(out string? path)) return path;
            }

            return null;
        }

        public async Task<IProjectAccessor> LoadProjectAsync(string projectFile, string configurationName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(projectFile)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(projectFile));

            string? msBuildToolsPath = this.LocateMsBuildToolsPath();
            if (string.IsNullOrWhiteSpace(msBuildToolsPath))
                throw new InvalidOperationException("Cannot detect the .NET SDK");

            // Environment.SetEnvironmentVariable("MsBuildToolsPath", msBuildToolsPath);

            var workspace = MSBuildWorkspace.Create();
            Project project = await workspace.OpenProjectAsync(projectFile, this.msBuildLogger, new Progress<ProjectLoadProgress>(), cancellationToken);
            Compilation? compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation == null) return ProjectAccessor.Empty;
            
            /* using var stream = new MemoryStream();
            compilation.Emit(stream, cancellationToken: cancellationToken); */

            /* var m = new MSBuildProjectLoader(workspace);
            var map = ProjectMap.Create();
            ImmutableArray<ProjectInfo> info = await m.LoadProjectInfoAsync(projectFile, map, cancellationToken: cancellationToken); */
            
            return new ProjectAccessor(project, configurationName);
        }
    }

    public sealed class ProjectAccessor : IProjectAccessor
    {
        public static readonly IProjectAccessor Empty = new ProjectAccessor();
        private readonly string? configurationName;

        private readonly Project? project;

        private ProjectAccessor()
        {
        }

        public ProjectAccessor(Project project, string configurationName)
        {
            if (string.IsNullOrWhiteSpace(configurationName)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(configurationName));
            this.project = project ?? throw new ArgumentNullException(nameof(project));
            this.configurationName = configurationName;
        }

        public void DeriveNuspec()
        {
            
        }
    }
}