namespace dotnet.nugit.Commands
{
    using Abstractions;
    using Microsoft.Extensions.Logging;

    public class ListEnvironmentVariablesCommand
    {
        private readonly IVariablesService variablesService;
        private readonly ILogger<ListEnvironmentVariablesCommand> logger;

        public ListEnvironmentVariablesCommand(
            IVariablesService variablesService,
            ILogger<ListEnvironmentVariablesCommand> logger)
        {
            this.variablesService = variablesService ?? throw new ArgumentNullException(nameof(variablesService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> ListEnvironmentVariablesAsync()
        {
            this.WriteVariables();

            return await Task.FromResult(0);
        }

        private void WriteVariables()
        {
            IEnumerable<string> variableNames = this.variablesService.GetVariableNames();
            List<string> sortedNames = variableNames.OrderBy(s => s).ToList();
            foreach (string variableName in sortedNames)
            {
                if (this.variablesService.TryGetVariable(variableName, out string? value))
                {
                    this.logger.LogDebug("Variable: {0}, Value: {1}", variableName, value);
                    Console.WriteLine($"{variableName}={value}");
                }
            }
        }
    }
}