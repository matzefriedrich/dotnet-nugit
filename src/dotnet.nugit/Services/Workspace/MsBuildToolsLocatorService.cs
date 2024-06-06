namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Reflection;
    using System.Runtime.Loader;
    using Abstractions;
    using Microsoft.Extensions.Logging;

    public sealed class MsBuildToolsLocatorService : IMsBuildToolsLocator, IDisposable
    {
        private readonly CustomAssemblyLoadContext assemblyLoadContext;
        private readonly IFileSystem fileSystem;
        private readonly ILogger<MsBuildToolsLocatorService> logger;
        private readonly IEnumerable<IMsBuildToolPathLocator> msBuildLocators;
        private bool initialized;
        private string? msBuildToolsPath;

        public MsBuildToolsLocatorService(
            IFileSystem fileSystem,
            IEnumerable<IMsBuildToolPathLocator> msBuildLocators,
            ILogger<MsBuildToolsLocatorService> logger)
        {
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this.msBuildLocators = msBuildLocators ?? throw new ArgumentNullException(nameof(msBuildLocators));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.assemblyLoadContext = new CustomAssemblyLoadContext();
        }

        public void Dispose()
        {
            AssemblyLoadContext.Default.Resolving -= this.DefaultOnResolving;
        }

        public string? LocateMsBuildToolsPath()
        {
            if (this.initialized) return this.msBuildToolsPath;

            foreach (IMsBuildToolPathLocator toolPathLocator in this.msBuildLocators)
            {
                if (!toolPathLocator.TryLocateMsBuildToolsPath(out string? resolvedPath))
                    continue;

                this.msBuildToolsPath = resolvedPath;
                this.initialized = true;
            }

            return this.msBuildToolsPath;
        }

        public void Initialize()
        {
            AssemblyLoadContext.Default.Resolving += this.DefaultOnResolving;

            string? buildToolsPath = this.LocateMsBuildToolsPath();
            if (buildToolsPath == null) return;
        }

        private Assembly? DefaultOnResolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            if (this.initialized == false || string.IsNullOrWhiteSpace(this.msBuildToolsPath)) return null;

            this.logger.LogDebug("Resolution of assembly {AssemblyName} has failed.", assemblyName.Name);

            string executionAssemblyLocation = Assembly.GetExecutingAssembly().Location;
            string? installationPath = this.fileSystem.Path.GetDirectoryName(executionAssemblyLocation);
            string?[] paths = { installationPath, this.msBuildToolsPath };
            foreach (string? p in paths)
            {
                Assembly? assembly = this.LoadAssemblyFromPath(p, assemblyName);
                if (assembly != null) return assembly;
            }

            return null;
        }

        private Assembly? LoadAssemblyFromPath(string? path, AssemblyName assemblyName)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            string assemblyPath = this.fileSystem.Path.Combine(path, $"{assemblyName.Name}.dll");
            if (this.fileSystem.File.Exists(assemblyPath))
            {
                this.logger.LogInformation("Loading {AssemblyName} from path: {Path}", assemblyName.Name, path);
                Assembly assembly = this.assemblyLoadContext.LoadFromAssemblyPath(assemblyPath);
                this.assemblyLoadContext.CacheAssembly(assemblyName, assembly);
                return assembly;
            }

            return null;
        }
    }
}