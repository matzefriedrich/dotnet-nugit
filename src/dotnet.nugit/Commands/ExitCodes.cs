namespace dotnet.nugit.Commands
{
    internal static class ExitCodes
    {
        public const int Ok = 0;
        public const int ErrLocalFeedNotFound = 101;
        public const int ErrCannotCreateFeed = 102;
        public const int ErrInvalidRepositoryReference = 111;
        public const int ErrCannotOpen = 112;
        public const int ErrNotAWorkspace = 201;
    }
}