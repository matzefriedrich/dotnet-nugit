﻿namespace dotnet.nugit.Commands
{
    using Abstractions;
    using Microsoft.Extensions.DependencyInjection;

    internal sealed class CommandsModule : IModule
    {
        public void LoadModule(IServiceCollection services)
        {
            services.AddTransient<ListEnvironmentVariablesCommand>();
            services.AddTransient<InitCommand>();
            services.AddTransient<AddPackagesFromRepositoryCommand>();
            services.AddTransient<RestorePackagesCommand>();
        }
    }
}