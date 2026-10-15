using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RiskyBusiness.Api.Infrastructure;

namespace RiskyBusiness.Api.Modules.Glossary;

public sealed record CreateGlossaryTermRequest(
    [Required, StringLength(GlossaryTerm.TermMaxLength)] string Term,
    [Required, StringLength(GlossaryTerm.DefinitionMaxLength)] string Definition);

public sealed record GlossaryTermResponse(Guid Id, string Term, string Definition, DateTimeOffset CreatedAt);

public static class GlossaryEndpoints
{
    public static IEndpointRouteBuilder MapGlossaryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/glossary").WithTags("Glossary");

        group.MapGet("/", ListTerms);
        group.MapGet("/{id:guid}", GetTerm);
        group.MapPost("/", CreateTerm);

        return app;
    }

    private static async Task<Ok<GlossaryTermResponse[]>> ListTerms(AppDbContext db, CancellationToken cancellationToken)
    {
        var terms = await db.GlossaryTerms
            .OrderBy(t => t.NormalizedTerm)
            .Select(t => new GlossaryTermResponse(t.Id, t.Term, t.Definition, t.CreatedAt))
            .ToArrayAsync(cancellationToken);

        return TypedResults.Ok(terms);
    }

    private static async Task<Results<Ok<GlossaryTermResponse>, NotFound>> GetTerm(
        Guid id, AppDbContext db, CancellationToken cancellationToken)
    {
        var term = await db.GlossaryTerms.FindAsync([id], cancellationToken);

        return term is null ? TypedResults.NotFound() : TypedResults.Ok(ToResponse(term));
    }

    private static async Task<Results<Created<GlossaryTermResponse>, ProblemHttpResult>> CreateTerm(
        CreateGlossaryTermRequest request, AppDbContext db, TimeProvider clock, CancellationToken cancellationToken)
    {
        var normalizedTerm = GlossaryTerm.Normalize(request.Term);
        if (await db.GlossaryTerms.AnyAsync(t => t.NormalizedTerm == normalizedTerm, cancellationToken))
        {
            return DuplicateTerm(request.Term);
        }

        var term = GlossaryTerm.Create(request.Term, request.Definition, clock.GetUtcNow());
        db.GlossaryTerms.Add(term);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Another request created the same term between the check above and this insert.
            return DuplicateTerm(request.Term);
        }

        return TypedResults.Created($"/api/glossary/{term.Id}", ToResponse(term));
    }

    private static GlossaryTermResponse ToResponse(GlossaryTerm term) =>
        new(term.Id, term.Term, term.Definition, term.CreatedAt);

    private static ProblemHttpResult DuplicateTerm(string term) =>
        TypedResults.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Glossary term already exists",
            detail: $"A glossary term matching '{term.Trim()}' already exists.");
}
