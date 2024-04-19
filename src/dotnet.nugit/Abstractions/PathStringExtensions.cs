namespace dotnet.nugit.Abstractions
{
    using System;
    using System.IO;
    using Resources;

    public static class PathStringExtensions
    {
        public static string SanitizedPathString(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(s));
            return s.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}