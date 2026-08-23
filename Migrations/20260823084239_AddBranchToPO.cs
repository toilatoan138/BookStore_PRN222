using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchToPO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "Purchase_Orders",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_Orders_branch_id",
                table: "Purchase_Orders",
                column: "branch_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_Orders_Branches_branch_id",
                table: "Purchase_Orders",
                column: "branch_id",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Orders_Branches_branch_id",
                table: "Purchase_Orders");

            migrationBuilder.DropIndex(
                name: "IX_Purchase_Orders_branch_id",
                table: "Purchase_Orders");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "Purchase_Orders");
        }
    }
}
