namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Microsoft.Build.Execution;
    using Microsoft.CodeAnalysis;
    using Resources;

    public sealed class ProjectAccessor : IProjectAccessor
    {
        public static readonly IProjectAccessor Empty = new ProjectAccessor();
        private readonly string? configurationName;

        private readonly Project? project;
        private readonly ProjectInstance? projectInstance;

        private ProjectAccessor()
        {
        }

        public ProjectAccessor(Project project, ProjectInstance? projectInstance, string configurationName)
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentNullException.ThrowIfNull(projectInstance);
            if (string.IsNullOrWhiteSpace(configurationName)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(configurationName));

            this.project = project;
            this.projectInstance = projectInstance;
            this.configurationName = configurationName;
        }

        public IProjectPackageMetadata DeriveNuspec()
        {
            if (this.projectInstance == null) return ProjectPackageMetadata.Empty();
            Dictionary<string, string?> map = ProjectPackageMetadata.GetPropertyNames()
                .Select(name => (name, this.projectInstance.Properties.SingleOrDefault(instance => PropertyFilter(instance, name))?.EvaluatedValue))
                .Where(tuple => string.IsNullOrWhiteSpace(tuple.EvaluatedValue) == false)
                .ToDictionary(tuple => (string) tuple.name, tuple => tuple.EvaluatedValue);
            
            var metadata = new ProjectPackageMetadata(map);
            metadata.AddDefaultValues(ProjectPackageMetadata.AssemblyTitle, this.project?.Name);
            return metadata;

            bool PropertyFilter(ProjectPropertyInstance instance, string propertyName)
            {
                return string.Compare(instance.Name, propertyName, StringComparison.InvariantCultureIgnoreCase) == 0;
            }
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