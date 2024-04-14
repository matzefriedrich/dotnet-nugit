namespace dotnet.nugit.Abstractions
{
    using Resources;

    public class LocalFeedInfo
    {
        public LocalFeedInfo(string name, string localPath)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(name));
            }

            if (string.IsNullOrWhiteSpace(localPath)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(localPath));

            this.Name = name;
            this.LocalPath = localPath;
        }

        public string Name { get; }
        public string LocalPath { get; }
    }
}