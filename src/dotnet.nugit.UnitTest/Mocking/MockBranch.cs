namespace dotnet.nugit.UnitTest.Mocking
{
    using LibGit2Sharp;

    internal sealed class MockBranch : Branch
    {
        public MockBranch(Commit tip)
        {
            this.Tip = tip;
        }

        public override Commit Tip { get; }
    }
}