namespace dotnet.nugit.Commands
{
    using Abstractions;

    public class ListEnvironmentVariablesCommand
    {
        private readonly IVariablesService variablesService;

        public ListEnvironmentVariablesCommand(IVariablesService variablesService)
        {
            this.variablesService = variablesService ?? throw new ArgumentNullException(nameof(variablesService));
        }

        public async Task<int> ListEnvironmentVariablesAsync()
        {
            this.WriteVariables();

            return await Task.FromResult(0);
        }

        private void WriteVariables()
        {
            List<string> variableNames = this.variablesService.GetVariableNames().OrderBy(s => s).ToList();
            foreach (string variableName in variableNames)
            {
                if (this.variablesService.TryGetVariable(variableName, out string? value))
                {
                    Console.WriteLine($"{variableName}={value}");
                }
            }
        }
    }
}