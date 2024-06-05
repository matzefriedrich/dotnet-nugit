namespace dotnet.nugit.Services.Workspace
{
    using System;
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

        public ProjectAccessor(Project project, ProjectInstance projectInstance, string configurationName)
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentNullException.ThrowIfNull(projectInstance);
            if (string.IsNullOrWhiteSpace(configurationName)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(configurationName));

            this.project = project;
            this.projectInstance = projectInstance;
            this.configurationName = configurationName;
        }

        public void DeriveNuspec()
        {
        }
    }
}