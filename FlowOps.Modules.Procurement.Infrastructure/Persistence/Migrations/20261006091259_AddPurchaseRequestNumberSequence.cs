using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowOps.Modules.Procurement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseRequestNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "purchase_request_number_seq",
                schema: "procurement");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "purchase_request_number_seq",
                schema: "procurement");
        }
    }
}
