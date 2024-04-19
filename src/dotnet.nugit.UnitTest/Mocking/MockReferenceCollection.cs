namespace dotnet.nugit.UnitTest.Mocking
{
    using LibGit2Sharp;

    internal sealed class MockReferenceCollection(IDictionary<string, Reference>? references = null) : ReferenceCollection
    {
        private readonly IDictionary<string, Reference> references = references ?? new Dictionary<string, Reference>();

        public override Reference this[string name] => this.references[name];

        public override IEnumerator<Reference> GetEnumerator()
        {
            return this.references.Values.GetEnumerator();
        }
    }
}