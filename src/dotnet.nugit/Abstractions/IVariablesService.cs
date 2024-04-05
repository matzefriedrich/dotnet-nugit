namespace dotnet.nugit.Abstractions
{
    public interface IVariablesService
    {
        IEnumerable<string> GetVariableNames();
        bool TryGetVariable(string name, out string? value);
    }
}