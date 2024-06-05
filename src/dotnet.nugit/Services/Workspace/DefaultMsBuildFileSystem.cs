namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Build.FileSystem;

    internal sealed class DefaultMsBuildFileSystem : MSBuildFileSystemBase
    {
        public override bool DirectoryExists(string path) => Directory.Exists(path);
        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) => Directory.EnumerateDirectories(path, searchPattern, searchOption);
        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) => Directory.EnumerateFiles(path, searchPattern, searchOption);
        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly) => Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);
        public override bool FileExists(string path) => File.Exists(path);
        public override bool FileOrDirectoryExists(string path) => File.Exists(path) || Directory.Exists(path);
        public override FileAttributes GetAttributes(string path) => File.GetAttributes(path);
        public override Stream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share) => new FileStream(path, mode, access, share);
        public override DateTime GetLastWriteTimeUtc(string path) => File.GetLastWriteTimeUtc(path);
        public override TextReader ReadFile(string path) => new StreamReader(path);
        public override byte[] ReadFileAllBytes(string path) => File.ReadAllBytes(path);
        public override string ReadFileAllText(string path) => File.ReadAllText(path);
    }
}