namespace dotnet.nugit.Abstractions
{
    using Resources;

    public abstract class VariableAccessor
    {
        protected VariableAccessor(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(name));

            this.Name = name;
        }

        public string Name { get; }
        public abstract string? Value();
    }
}