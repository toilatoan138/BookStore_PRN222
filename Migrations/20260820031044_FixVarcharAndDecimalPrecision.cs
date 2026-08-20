using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Migrations
{
    /// <inheritdoc />
    public partial class FixVarcharAndDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                table: "Wallet_History",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "min_order_value",
                table: "Vouchers",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,0)");

            migrationBuilder.AlterColumn<decimal>(
                name: "max_discount",
                table: "Vouchers",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "discount_amount",
                table: "Vouchers",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,0)");

            migrationBuilder.AlterColumn<string>(
                name: "return_method",
                table: "ReturnRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "refund_preference",
                table: "ReturnRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "refund_amount",
                table: "RefundTransactions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "processed_by",
                table: "RefundTransactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "shipping_fee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "discount_amount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "amount",
                table: "Wallet_History",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "min_order_value",
                table: "Vouchers",
                type: "decimal(18,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<double>(
                name: "max_discount",
                table: "Vouchers",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "discount_amount",
                table: "Vouchers",
                type: "decimal(18,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "return_method",
                table: "ReturnRequests",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "refund_preference",
                table: "ReturnRequests",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "refund_amount",
                table: "RefundTransactions",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "processed_by",
                table: "RefundTransactions",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "shipping_fee",
                table: "Orders",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "discount_amount",
                table: "Orders",
                type: "decimal(18,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
