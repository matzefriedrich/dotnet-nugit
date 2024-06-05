using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using dotnet.nugit.Abstractions;
using Microsoft.Build.Evaluation.Context;
using Microsoft.Build.Execution;
using Microsoft.Build.FileSystem;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;

namespace dotnet.nugit.Services.Workspace;

internal sealed class Fs : MSBuildFileSystemBase
{
}

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

        var workspaceLogger = loggerFactory.CreateLogger<ProjectWorkspaceLogger>();
        msBuildLogger = new ProjectWorkspaceLogger(workspaceLogger);
    }

    public string? LocateMsBuildToolsPath()
    {
        foreach (var toolPathLocator in msBuildLocators)
            if (toolPathLocator.TryLocateMsBuildToolsPath(out var path))
                return path;

        return null;
    }

    public async Task<IProjectAccessor> LoadProjectAsync(string projectFile, string configurationName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(projectFile))
            throw new ArgumentException(Resources.Resources.ArgumentException_Value_cannot_be_null_or_whitespace,
                nameof(projectFile));

        var msBuildToolsPath = LocateMsBuildToolsPath();
        if (string.IsNullOrWhiteSpace(msBuildToolsPath))
            throw new InvalidOperationException("Cannot detect the .NET SDK");

        // Environment.SetEnvironmentVariable("MsBuildToolsPath", msBuildToolsPath);

        var workspace = MSBuildWorkspace.Create();
        var project = await workspace.OpenProjectAsync(projectFile, msBuildLogger, new Progress<ProjectLoadProgress>(),
            cancellationToken);
        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation == null) return ProjectAccessor.Empty;

        /* using var stream = new MemoryStream();
        compilation.Emit(stream, cancellationToken: cancellationToken); */

        /* var m = new MSBuildProjectLoader(workspace);
        var map = ProjectMap.Create();
        ImmutableArray<ProjectInfo> info = await m.LoadProjectInfoAsync(projectFile, map, cancellationToken: cancellationToken); */

        await using Stream input = File.OpenRead(projectFile);
        using var reader = XmlReader.Create(input);
        var p = new Microsoft.Build.Evaluation.Project(reader);
        const ProjectInstanceSettings settings = ProjectInstanceSettings.Immutable;
        MSBuildFileSystemBase fs = new Fs();
        var projectInstance = p.CreateProjectInstance(settings, EvaluationContext.Create(EvaluationContext.SharingPolicy.Isolated, fs));

        return new ProjectAccessor(project, projectInstance, configurationName);
    }
}

public sealed class ProjectAccessor : IProjectAccessor
{
    public static readonly IProjectAccessor Empty = new ProjectAccessor();
    
    private readonly ProjectInstance? _projectPropertiesEvaluationInstance;
    private readonly Project? project;
    private readonly string? configurationName;

    private ProjectAccessor()
    {
    }

    public ProjectAccessor(
        Project codeAnalysisProject,
        ProjectInstance projectPropertiesEvaluationInstance,
        string configurationName)
    {
        if (string.IsNullOrWhiteSpace(configurationName)) throw new ArgumentException(Resources.Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(configurationName));
        this.project = codeAnalysisProject ?? throw new ArgumentNullException(nameof(codeAnalysisProject));
        this._projectPropertiesEvaluationInstance = projectPropertiesEvaluationInstance ?? throw new ArgumentNullException(nameof(projectPropertiesEvaluationInstance));
        this.configurationName = configurationName;
    }

    public void DeriveNuspec()
    {
    }
}