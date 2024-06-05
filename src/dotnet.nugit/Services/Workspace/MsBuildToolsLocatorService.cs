namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.Collections.Generic;
    using Abstractions;

    public sealed class MsBuildToolsLocatorService(IEnumerable<IMsBuildToolPathLocator> msBuildLocators) : IMsBuildToolsLocator
    {
        private readonly IEnumerable<IMsBuildToolPathLocator> msBuildLocators = msBuildLocators ?? throw new ArgumentNullException(nameof(msBuildLocators));
        private bool initialized;
        private string? path;

        public string? LocateMsBuildToolsPath()
        {
            if (this.initialized) return this.path;

            foreach (IMsBuildToolPathLocator toolPathLocator in this.msBuildLocators)
            {
                if (!toolPathLocator.TryLocateMsBuildToolsPath(out string? path)) 
                    continue;
                
                this.path = path;
                this.initialized = true;
            }

            return this.path;
        }

        public void Initialize()
        {
            this.LocateMsBuildToolsPath();
        }
    }
}