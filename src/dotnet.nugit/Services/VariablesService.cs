namespace dotnet.nugit.Services
{
    using Abstractions;
    using Resources;

    public sealed class VariablesService : IVariablesService
    {
        private readonly IEnumerable<VariableAccessor> variableAccessors;

        public VariablesService(IEnumerable<VariableAccessor> variableAccessors)
        {
            this.variableAccessors = variableAccessors ?? throw new ArgumentNullException(nameof(variableAccessors));
        }

        public IEnumerable<string> GetVariableNames()
        {
            return this.variableAccessors.Select(accessor => accessor.Name).ToList().AsReadOnly();
        }

        public bool TryGetVariable(string name, out string? value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(name));

            string lookupName = name.ToLowerInvariant();
            IDictionary<string, string> variables = this.GetCurrentProcessVariables();
            return variables.TryGetValue(lookupName, out value);
        }

        private IDictionary<string, string> GetCurrentProcessVariables()
        {
            var processVariables = new Dictionary<string, string>();
            foreach (VariableAccessor variable in this.variableAccessors)
            {
                string variableName = variable.Name;
                string variableLookupName = variableName.ToLowerInvariant();
                processVariables[variableLookupName] = variable.Value() ?? string.Empty;
                string? value = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)?.Trim();
                if (string.IsNullOrWhiteSpace(value) == false) processVariables[variableLookupName] = value;
            }

            return processVariables;
        }
    }
}