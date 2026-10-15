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
    public void Create_truncates_timestamp_to_database_precision()
    {
        var withSubMicrosecondTicks = new DateTimeOffset(2026, 10, 15, 20, 7, 6, TimeSpan.Zero).AddTicks(4_701_109);

        var term = GlossaryTerm.Create("Binder", "A delegated authority agreement.", withSubMicrosecondTicks);

        Assert.Equal(withSubMicrosecondTicks.AddTicks(-9), term.CreatedAt);
    }

    [Fact]
    public void Terms_differing_only_by_case_and_whitespace_normalize_identically()
    {
        Assert.Equal(GlossaryTerm.Normalize("Coverholder"), GlossaryTerm.Normalize(" coverHOLDER "));
    }
}
