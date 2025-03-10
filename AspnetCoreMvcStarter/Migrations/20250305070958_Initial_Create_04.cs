using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcStarter.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Create_04 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoicesId",
                table: "SmsLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SmslogId",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_InvoicesId",
                table: "SmsLogs",
                column: "InvoicesId");

            migrationBuilder.AddForeignKey(
                name: "FK_SmsLogs_Invoices_InvoicesId",
                table: "SmsLogs",
                column: "InvoicesId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsLogs_Invoices_InvoicesId",
                table: "SmsLogs");

            migrationBuilder.DropIndex(
                name: "IX_SmsLogs_InvoicesId",
                table: "SmsLogs");

            migrationBuilder.DropColumn(
                name: "InvoicesId",
                table: "SmsLogs");

            migrationBuilder.DropColumn(
                name: "SmslogId",
                table: "Invoices");
        }
    }
}
