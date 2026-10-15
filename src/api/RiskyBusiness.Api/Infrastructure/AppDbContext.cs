using Microsoft.EntityFrameworkCore;
using RiskyBusiness.Api.Modules.Glossary;

namespace RiskyBusiness.Api.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GlossaryTerm> GlossaryTerms => Set<GlossaryTerm>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
