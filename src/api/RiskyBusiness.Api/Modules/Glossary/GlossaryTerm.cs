using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RiskyBusiness.Api.Modules.Glossary;

public sealed class GlossaryTerm
{
    public const int TermMaxLength = 100;
    public const int DefinitionMaxLength = 2000;

    // Used by EF Core; application code should call Create.
    private GlossaryTerm(Guid id, string term, string normalizedTerm, string definition, DateTimeOffset createdAt)
    {
        Id = id;
        Term = term;
        NormalizedTerm = normalizedTerm;
        Definition = definition;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Term { get; private set; }

    /// <summary>Upper-cased term used to enforce case-insensitive uniqueness.</summary>
    public string NormalizedTerm { get; private set; }

    public string Definition { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static GlossaryTerm Create(string term, string definition, DateTimeOffset createdAt)
    {
        var trimmedTerm = term.Trim();
        return new GlossaryTerm(Guid.CreateVersion7(createdAt), trimmedTerm, Normalize(trimmedTerm), definition.Trim(), createdAt);
    }

    public static string Normalize(string term) => term.Trim().ToUpperInvariant();
}

internal sealed class GlossaryTermConfiguration : IEntityTypeConfiguration<GlossaryTerm>
{
    public void Configure(EntityTypeBuilder<GlossaryTerm> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Term).HasMaxLength(GlossaryTerm.TermMaxLength);
        builder.Property(t => t.NormalizedTerm).HasMaxLength(GlossaryTerm.TermMaxLength);
        builder.Property(t => t.Definition).HasMaxLength(GlossaryTerm.DefinitionMaxLength);
        builder.HasIndex(t => t.NormalizedTerm).IsUnique();
    }
}
