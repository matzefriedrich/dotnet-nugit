namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Abstractions;
    using Microsoft.Build.Evaluation.Context;
    using Microsoft.Build.Execution;
    using Microsoft.Build.FileSystem;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;
    using Microsoft.Extensions.Logging;

    public class ProjectWorkspaceManager : IProjectWorkspaceManager
    {
        private readonly ILogger<ProjectWorkspaceManager> logger;
        private readonly ProjectWorkspaceLogger msBuildLogger;
        private readonly IMsBuildToolsLocator msBuildToolsLocator;
        private readonly IFileSystem fileSystem;

        public ProjectWorkspaceManager(
            IMsBuildToolsLocator msBuildToolsLocator,
            IFileSystem fileSystem,
            ILoggerFactory loggerFactory,
            ILogger<ProjectWorkspaceManager> logger)
        {
            this.msBuildToolsLocator = msBuildToolsLocator ?? throw new ArgumentNullException(nameof(msBuildToolsLocator));
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ILogger<ProjectWorkspaceLogger> workspaceLogger = loggerFactory.CreateLogger<ProjectWorkspaceLogger>();
            this.msBuildLogger = new ProjectWorkspaceLogger(workspaceLogger);
        }

        public async Task<IDotNetProject> LoadProjectAsync(string projectFile, string configurationName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(projectFile)) throw new ArgumentException(Resources.Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(projectFile));

            string? msBuildToolsPath = this.msBuildToolsLocator.LocateMsBuildToolsPath();
            if (string.IsNullOrWhiteSpace(msBuildToolsPath))
                throw new InvalidOperationException("Cannot detect the .NET SDK");

            Environment.SetEnvironmentVariable("MsBuildToolsPath", msBuildToolsPath);

            var workspace = MSBuildWorkspace.Create();
            Project project = await workspace.OpenProjectAsync(projectFile, this.msBuildLogger, new Progress<ProjectLoadProgress>(), cancellationToken);
            Compilation? compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation == null) return DotNetProject.Empty;

            /* using var stream = new MemoryStream();
            compilation.Emit(stream, cancellationToken: cancellationToken); */

            /* var m = new MSBuildProjectLoader(workspace);
            var map = ProjectMap.Create();
            ImmutableArray<ProjectInfo> info = await m.LoadProjectInfoAsync(projectFile, map, cancellationToken: cancellationToken); */

            // TODO: derive nuspec file location
            string projectNuspecFile = Path.Combine(Path.GetDirectoryName(projectFile)!, $"{Path.GetFileNameWithoutExtension(projectFile)}.nuspec");


            await using Stream input = File.OpenRead(projectFile);
            using var reader = XmlReader.Create(input);
            var p = new Microsoft.Build.Evaluation.Project(reader);
            const ProjectInstanceSettings settings = ProjectInstanceSettings.Immutable;
            MSBuildFileSystemBase fs = new DefaultMsBuildFileSystem();
            ProjectInstance? projectInstance = p.CreateProjectInstance(settings, EvaluationContext.Create(EvaluationContext.SharingPolicy.Shared, fs));

            return new DotNetProject(this.fileSystem, project, projectInstance);
        }
    }
}