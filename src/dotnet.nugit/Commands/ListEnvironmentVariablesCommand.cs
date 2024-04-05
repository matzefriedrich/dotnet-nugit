namespace dotnet.nugit.Commands
{
    public static class Variables
    {
        public static readonly string NugitHomeVariableName = "NUGIT_HOME";
        public static readonly Func<string> NugitHome = () => "";
    }

    public class ListEnvironmentVariablesCommand
    {
        private static readonly IDictionary<string, Func<string?>> variables = new Dictionary<string, Func<string?>>
        {
            [Variables.NugitHomeVariableName] = Variables.NugitHome
        };

        public async Task<int> ListEnvironmentVariablesAsync()
        {
            IDictionary<string, string> processVariables = GetCurrentProcessVariables();

            WriteVariables(processVariables);

            return await Task.FromResult(0);
        }

        private static void WriteVariables(IDictionary<string, string> processVariables)
        {
            List<string> keys = processVariables.Keys.OrderBy(s => s).ToList();
            foreach (string variableName in keys)
            {
                string value = processVariables[variableName];
                Console.WriteLine($"{variableName}={value}");
            }
        }

        private static IDictionary<string, string> GetCurrentProcessVariables()
        {
            var processVariables = new Dictionary<string, string>();
            foreach ((string variable, Func<string?> defaultValue) in variables)
            {
                processVariables[variable] = defaultValue?.Invoke() ?? string.Empty;
                string? value = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Process)?.Trim();
                if (string.IsNullOrWhiteSpace(value) == false) processVariables[variable] = value;
            }

            return processVariables;
        }
    }
}