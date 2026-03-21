using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicePulseMonitor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCurrentStatusAndNullableAlertUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "current_status",
                table: "services",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Healthy");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_guid",
                table: "alerts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "current_status",
                table: "services");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_guid",
                table: "alerts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
