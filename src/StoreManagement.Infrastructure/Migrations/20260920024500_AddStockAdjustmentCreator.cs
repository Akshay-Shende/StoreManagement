using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StoreManagement.Infrastructure.Data;

#nullable disable

namespace StoreManagement.Migrations;

[DbContext(typeof(StoreDbContext))]
[Migration("20260920024500_AddStockAdjustmentCreator")]
public partial class AddStockAdjustmentCreator : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "StockAdjustments",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: false,
            defaultValue: "system");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "StockAdjustments");
    }
}
