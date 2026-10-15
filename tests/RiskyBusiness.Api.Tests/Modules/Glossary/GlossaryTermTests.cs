using RiskyBusiness.Api.Modules.Glossary;

namespace RiskyBusiness.Api.Tests.Modules.Glossary;

public sealed class GlossaryTermTests
{
    [Fact]
    public void Create_trims_input_and_normalizes_term_for_uniqueness()
    {
        var term = GlossaryTerm.Create("  Managing General Agent ", " An intermediary with delegated authority. ", DateTimeOffset.UnixEpoch);

        Assert.Equal("Managing General Agent", term.Term);
        Assert.Equal("MANAGING GENERAL AGENT", term.NormalizedTerm);
        Assert.Equal("An intermediary with delegated authority.", term.Definition);
    }

    [Fact]
    public void Terms_differing_only_by_case_and_whitespace_normalize_identically()
    {
        Assert.Equal(GlossaryTerm.Normalize("Coverholder"), GlossaryTerm.Normalize(" coverHOLDER "));
    }
}
