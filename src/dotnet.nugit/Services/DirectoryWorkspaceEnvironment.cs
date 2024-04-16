namespace dotnet.nugit.Services
{
    using System.Text;
    using Abstractions;

    public abstract class DirectoryWorkspaceEnvironment : IWorkspaceEnvironment
    {
        private const string WorkspaceConfigurationFileName = ".nugit";
        
        private static readonly Encoding ConfigurationFileEncoding = Encoding.UTF8;
        
        public TextReader CreateConfigurationFileReader()
        {
            string nugitFile = this.WorkspaceConfigurationFilePath();
            if (File.Exists(nugitFile) == false)
                return StreamReader.Null;

            Stream stream = new FileStream(nugitFile, FileMode.Open, FileAccess.Read, FileShare.None);
            return new StreamReader(stream, ConfigurationFileEncoding);
        }

        public TextWriter GetConfigurationFileWriter()
        {
            string nugitFile = this.WorkspaceConfigurationFilePath();
            Stream stream = new FileStream(nugitFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            stream.SetLength(0);

            return new StreamWriter(stream, ConfigurationFileEncoding, 4096);
        }

        protected abstract string DirectoryPath();

        public string WorkspaceConfigurationFilePath()
        {
            return Path.Combine(this.DirectoryPath(), WorkspaceConfigurationFileName);
        }
    }
}