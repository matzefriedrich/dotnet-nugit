namespace dotnet.nugit.Services
{
    using Abstractions;

    internal sealed class NugitHomeVariableAccessor() : VariableAccessor(ApplicationVariableNames.NugitHome)
    {
        public override string? Value()
        {
            string? nugitHomeVariableValue = Environment.GetEnvironmentVariable(this.Name);
            if (string.IsNullOrWhiteSpace(nugitHomeVariableValue))
            {
                string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                nugitHomeVariableValue = Path.Combine(homePath, ".nugit");
            }

            return nugitHomeVariableValue;
        }
    }
}