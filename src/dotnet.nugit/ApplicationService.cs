namespace dotnet.nugit
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Extensions;
    using System.Threading;
    using Commands;

    internal sealed class ApplicationService : IDisposable
    {
        private readonly AddPackagesFromRepositoryCommand addPackagesFromRepositoryCommand;
        private readonly CommandLineApplication app;

        private readonly CancellationTokenSource cancellationTokenSource = new();
        private readonly InitCommand initCommand;
        private readonly ListEnvironmentVariablesCommand listEnvironmentVariablesCommand;
        private readonly RestorePackagesCommand restorePackagesCommand;

        public ApplicationService(
            CommandLineApplication app,
            ListEnvironmentVariablesCommand listEnvironmentVariablesCommand,
            InitCommand initCommand,
            AddPackagesFromRepositoryCommand addPackagesFromRepositoryCommand,
            RestorePackagesCommand restorePackagesCommand)
        {
            this.app = app ?? throw new ArgumentNullException(nameof(app));
            this.listEnvironmentVariablesCommand = listEnvironmentVariablesCommand ?? throw new ArgumentNullException(nameof(listEnvironmentVariablesCommand));
            this.initCommand = initCommand ?? throw new ArgumentNullException(nameof(initCommand));
            this.addPackagesFromRepositoryCommand = addPackagesFromRepositoryCommand ?? throw new ArgumentNullException(nameof(addPackagesFromRepositoryCommand));
            this.restorePackagesCommand = restorePackagesCommand ?? throw new ArgumentNullException(nameof(restorePackagesCommand));

            this.Initialize();
        }

        public void Dispose()
        {
            this.cancellationTokenSource.Dispose();
        }

        private void Initialize()
        {
            this.InitializeListEnvironmentVariablesCommand();
            this.InitializeInitRepositoryCommand();
            this.InitializeAddPackagesFromRepositoryCommand();
            this.InitializeRestorePackagesCommand();
        }

        private void InitializeRestorePackagesCommand()
        {
            this.app.Command("restore", "Restores all packages from referenced repositories.", restore =>
            {
                restore
                    .Option<bool?>("--reinstall", "Forces a fresh checkout of referenced repositories.")
                    .OnExecute(async (bool? reinstall) => await this.restorePackagesCommand.RestoreWorkspacePackagesAsync(reinstall ?? false, this.cancellationTokenSource.Token));
            });
        }

        private void InitializeAddPackagesFromRepositoryCommand()
        {
            this.app.Command("add", "Builds a package from a referenced repository and publishes it to the local feed.", add =>
            {
                add
                    .Option<string>("--repository", "The repository URL.", ArgumentArity.ExactlyOne)
                    .Option<bool?>("--head-only", "Builds a single package from the head instead of all available releases (tag references).")
                    .OnExecute(async (string repository, bool? headOnly) =>
                        await this.addPackagesFromRepositoryCommand.ProcessRepositoryAsync(repository, headOnly ?? false, this.cancellationTokenSource.Token));
            });
        }

        private void InitializeInitRepositoryCommand()
        {
            this.app.Command("init", "Initializes a new local repository.", init =>
            {
                init
                    .Option<bool?>("--local", "Adds local copies to current workspace.")
                    .OnExecute(async (bool? copyLocal) => await this.initCommand.InitializeNewRepositoryAsync(copyLocal ?? false));
            });
        }

        private void InitializeListEnvironmentVariablesCommand()
        {
            this.app.Command("env", "Lists environment variables and their values.", env => { env.OnExecute(this.listEnvironmentVariablesCommand.ListEnvironmentVariablesAsync); });
        }

        public int Execute(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);
            return this.app.Execute(args);
        }
    }
}