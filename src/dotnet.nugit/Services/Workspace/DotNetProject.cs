namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using Abstractions;
    using Microsoft.Build.Execution;
    using Microsoft.CodeAnalysis;

    public sealed class DotNetProject : IDotNetProject
    {
        public static readonly DotNetProject Empty = new();

        private readonly IFileSystem? fileSystem;
        private readonly Project? project;
        private readonly ProjectInstance? projectInstance;

        public DotNetProject(
            IFileSystem fileSystem,
            Project project,
            ProjectInstance? projectInstance)
        {
            ArgumentNullException.ThrowIfNull(fileSystem);
            ArgumentNullException.ThrowIfNull(project);
            ArgumentNullException.ThrowIfNull(projectInstance);

            this.fileSystem = fileSystem;
            this.project = project;
            this.projectInstance = projectInstance;
        }

        private DotNetProject()
        {
        }

        public IProjectPackageMetadata DerivePackageSpec()
        {
            if (this.projectInstance == null) return ProjectPackageMetadata.Empty();
            Dictionary<string, string?> map = ProjectPackageMetadata.GetPropertyNames()
                .Select(name => (name, this.projectInstance.Properties.SingleOrDefault(instance => PropertyFilter(instance, name))?.EvaluatedValue))
                .Where(tuple => string.IsNullOrWhiteSpace(tuple.EvaluatedValue) == false)
                .ToDictionary(tuple => (string)tuple.name, tuple => tuple.EvaluatedValue);

            var metadata = new ProjectPackageMetadata(map);
            metadata.AddDefaultValues(ProjectPackageMetadata.AssemblyTitle, this.project?.Name);
            return metadata;

            bool PropertyFilter(ProjectPropertyInstance instance, string propertyName)
            {
                return string.Compare(instance.Name, propertyName, StringComparison.InvariantCultureIgnoreCase) == 0;
            }
        }

        /// <summary>
        ///     Retrieves a value indicating the path to the project file, or null if there is no project file.
        /// </summary>
        public string? GetProjectFilePath()
        {
            return this.project?.FilePath;
        }

        public string? GetNuspecBasePath()
        {
            string? projectFilePath = this.GetProjectFilePath();
            return this.fileSystem?.Path.GetDirectoryName(projectFilePath);
        }

        public string? GetNuspecFilePath()
        {
            string? projectFilePath = this.GetProjectFilePath();
            string? nuspecFileName = this.fileSystem?.Path.ChangeExtension(projectFilePath, "nuspec");
            string? nuspecBasePath = this.GetNuspecBasePath();

            if (string.IsNullOrWhiteSpace(nuspecBasePath) || string.IsNullOrWhiteSpace(nuspecFileName))
                return null;

            return this.fileSystem?.Path.Combine(nuspecBasePath, nuspecFileName);
        }
    }

    public sealed class ProjectPropertyName(string name)
    {
        private readonly string name = name;

        public static implicit operator ProjectPropertyName(string name)
        {
            return new ProjectPropertyName(name.ToLowerInvariant());
        }

        public static implicit operator string(ProjectPropertyName p)
        {
            return p.name;
        }
    }
}