namespace dotnet.nugit.Abstractions
{
    using Resources;

    public class LocalFeedInfo
    {
        public LocalFeedInfo(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(name));
            }

            this.Name = name;
        }

        public string Name { get; }
        public string? LocalPath { get; init; }
    }
}