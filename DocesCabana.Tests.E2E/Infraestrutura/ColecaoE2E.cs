namespace DocesCabana.Tests.E2E.Infraestrutura;

[CollectionDefinition(Nome)]
public class ColecaoE2E : ICollectionFixture<FixtureE2E>
{
    public const string Nome = "E2E";
}
