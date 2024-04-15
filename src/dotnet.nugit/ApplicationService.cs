namespace dotnet.nugit
{
    using System.CommandLine;
    using System.CommandLine.Extensions;
    using Commands;

    internal sealed class ApplicationService : IDisposable
    {
        private readonly AddPackagesFromRepositoryCommand addPackagesFromRepositoryCommand;
        private readonly CommandLineApplication app;

        private readonly CancellationTokenSource cancellationTokenSource = new();
        private readonly InitCommand initCommand;
        private readonly ListEnvironmentVariablesCommand listEnvironmentVariablesCommand;
        private readonly GreetingCommand sampleCommand;

        public ApplicationService(
            CommandLineApplication app,
            ListEnvironmentVariablesCommand listEnvironmentVariablesCommand,
            InitCommand initCommand,
            AddPackagesFromRepositoryCommand addPackagesFromRepositoryCommand,
            GreetingCommand sampleCommand)
        {
            this.app = app ?? throw new ArgumentNullException(nameof(app));
            this.listEnvironmentVariablesCommand = listEnvironmentVariablesCommand ?? throw new ArgumentNullException(nameof(listEnvironmentVariablesCommand));
            this.initCommand = initCommand ?? throw new ArgumentNullException(nameof(initCommand));
            this.addPackagesFromRepositoryCommand = addPackagesFromRepositoryCommand ?? throw new ArgumentNullException(nameof(addPackagesFromRepositoryCommand));
            this.sampleCommand = sampleCommand ?? throw new ArgumentNullException(nameof(sampleCommand));

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

            this.app.Command("greeting", "Greets the specified person.", greeting =>
            {
                greeting.Option<string>("--name", "The person´s name.", ArgumentArity.ExactlyOne)
                    .Option<bool>("--polite")
                    .OnExecute(async (string name, bool polite) => await this.sampleCommand.GreetAsync(name, polite));
            });
        }

        private void InitializeAddPackagesFromRepositoryCommand()
        {
            this.app.Command("add", "Builds a package from a referenced repository and publishes it to the local feed.", add =>
            {
                add
                    .Option<string>("--repository", "The repository URL.", ArgumentArity.ExactlyOne)
                    .Option<bool>("--head-only", "Builds a single package from the head instead of all available releases (tag references).")
                    .OnExecute(async (string repository, bool headOnly) =>
                        await this.addPackagesFromRepositoryCommand.ProcessRepositoryAsync(repository, headOnly, this.cancellationTokenSource.Token));
            });
        }

        private void InitializeInitRepositoryCommand()
        {
            this.app.Command("init", "Initializes a new local repository.", init =>
            {
                init
                    .Option<bool>("--local", "Adds local copies to current workspace.")
                    .OnExecute(async (bool copyLocal) => await this.initCommand.InitializeNewRepositoryAsync(copyLocal));
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