namespace dotnet.nugit.Abstractions
{
    using System.Collections.Generic;

    public interface IVariablesService
    {
        IEnumerable<string> GetVariableNames();
        bool TryGetVariable(string name, out string? value);
    }
}