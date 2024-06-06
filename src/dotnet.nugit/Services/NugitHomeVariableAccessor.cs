namespace dotnet.nugit.Services
{
    using System;
    using System.IO.Abstractions;
    using Abstractions;

    internal sealed class NugitHomeVariableAccessor(IFileSystem fileSystem) : VariableAccessor(ApplicationVariableNames.NugitHome)
    {
        private readonly IFileSystem fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

        public override string Value()
        {
            string? nugitHomeVariableValue = Environment.GetEnvironmentVariable(this.Name);
            if (string.IsNullOrWhiteSpace(nugitHomeVariableValue))
            {
                string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                nugitHomeVariableValue = this.fileSystem.Path.Combine(homePath, ".nugit");
            }

            return nugitHomeVariableValue;
        }
    }
}