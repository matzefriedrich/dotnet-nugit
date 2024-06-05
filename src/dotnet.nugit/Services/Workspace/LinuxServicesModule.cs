namespace dotnet.nugit.Services.Workspace
{
    using System;
    using Abstractions;
    using Microsoft.Extensions.DependencyInjection;

    internal sealed class LinuxServicesModule : IModule
    {
        public void LoadModule(IServiceCollection services)
        {
            if (OperatingSystem.IsLinux() == false)
                return;
            
            services.AddTransient<IMsBuildToolPathLocator, NetSdkToolPathLocator>();
        }
    }
}