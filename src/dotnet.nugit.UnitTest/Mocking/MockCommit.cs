namespace dotnet.nugit.UnitTest.Mocking
{
    using LibGit2Sharp;

    internal sealed class MockCommit : Commit
    {
        public MockCommit(string sha)
        {
            this.Sha = sha;
        }
        
        public override string Sha { get; }
    }
}