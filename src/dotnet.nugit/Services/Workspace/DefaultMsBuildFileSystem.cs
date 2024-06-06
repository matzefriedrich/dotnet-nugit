namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Build.FileSystem;

    internal sealed class DefaultMsBuildFileSystem : MSBuildFileSystemBase
    {
        public override bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return Directory.EnumerateDirectories(path, searchPattern, searchOption);
        }

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return Directory.EnumerateFiles(path, searchPattern, searchOption);
        }

        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);
        }

        public override bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public override bool FileOrDirectoryExists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        public override FileAttributes GetAttributes(string path)
        {
            return File.GetAttributes(path);
        }

        public override Stream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(path, mode, access, share);
        }

        public override DateTime GetLastWriteTimeUtc(string path)
        {
            return File.GetLastWriteTimeUtc(path);
        }

        public override TextReader ReadFile(string path)
        {
            return new StreamReader(path);
        }

        public override byte[] ReadFileAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public override string ReadFileAllText(string path)
        {
            return File.ReadAllText(path);
        }
    }
}