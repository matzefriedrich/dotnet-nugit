namespace dotnet.nugit.Commands
{
    using Abstractions;
    using Microsoft.Extensions.Logging;
    using static ExitCodes;

    public class ListEnvironmentVariablesCommand(
        IVariablesService variablesService,
        ILogger<ListEnvironmentVariablesCommand> logger)
    {
        private readonly ILogger<ListEnvironmentVariablesCommand> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IVariablesService variablesService = variablesService ?? throw new ArgumentNullException(nameof(variablesService));

        public async Task<int> ListEnvironmentVariablesAsync()
        {
            this.WriteVariables();

            return await Task.FromResult(Ok);
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