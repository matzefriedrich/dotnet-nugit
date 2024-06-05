namespace dotnet.nugit.Services.Workspace
{
    using System;
    using Abstractions;

    internal sealed class VisualStudioToolPathLocator : IMsBuildToolPathLocator
    {
        public bool TryLocateMsBuildToolsPath(out string? path)
        {
            path = null;
            if (OperatingSystem.IsWindows() == false)
                return false;

            // TODO: 

            return true;
        }
    }
}