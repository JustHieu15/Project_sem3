using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcStarter.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Create_06 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicesItems_Services_ServiceId",
                table: "InvoicesItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoicesItems_ServiceId",
                table: "InvoicesItems");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "InvoicesItems");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "InvoicesItems");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "InvoicesItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "InvoicesItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "Weight",
                table: "InvoicesItems",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "BarCode",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ServicesId",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ServicesId",
                table: "Invoices",
                column: "ServicesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Services_ServicesId",
                table: "Invoices",
                column: "ServicesId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Services_ServicesId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ServicesId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "InvoicesItems");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "InvoicesItems");

            migrationBuilder.DropColumn(
                name: "BarCode",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ServicesId",
                table: "Invoices");

            migrationBuilder.AlterColumn<float>(
                name: "Quantity",
                table: "InvoicesItems",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "InvoicesItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "InvoicesItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesItems_ServiceId",
                table: "InvoicesItems",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicesItems_Services_ServiceId",
                table: "InvoicesItems",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
