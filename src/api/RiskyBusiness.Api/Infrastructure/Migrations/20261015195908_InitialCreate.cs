using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiskyBusiness.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "glossary_terms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    term = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalized_term = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    definition = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_glossary_terms", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_glossary_terms_normalized_term",
                table: "glossary_terms",
                column: "normalized_term",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "glossary_terms");
        }
    }
}
