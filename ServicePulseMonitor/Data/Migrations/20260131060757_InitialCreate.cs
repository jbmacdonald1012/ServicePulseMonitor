using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ServicePulseMonitor.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    service_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    service_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    base_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    registered_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    last_seen_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.service_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    access_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_guid);
                });

            migrationBuilder.CreateTable(
                name: "alert_rules",
                columns: table => new
                {
                    rule_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    service_id = table.Column<long>(type: "bigint", nullable: false),
                    rule_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    threshold = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    log_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    notification_channel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_rules", x => x.rule_id);
                    table.ForeignKey(
                        name: "FK_alert_rules_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "health_checks",
                columns: table => new
                {
                    health_check_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    service_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    response_time_ms = table.Column<int>(type: "integer", nullable: true),
                    checked_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    details = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_checks", x => x.health_check_id);
                    table.CheckConstraint("CK_HealthCheck_Status", "status IN ('Healthy', 'Degraded', 'Unhealthy')");
                    table.ForeignKey(
                        name: "FK_health_checks_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_dependencies",
                columns: table => new
                {
                    dependency_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    service_id = table.Column<long>(type: "bigint", nullable: false),
                    depends_on_service_id = table.Column<long>(type: "bigint", nullable: false),
                    discovered_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_dependencies", x => x.dependency_id);
                    table.ForeignKey(
                        name: "FK_service_dependencies_services_depends_on_service_id",
                        column: x => x.depends_on_service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_service_dependencies_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    alert_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    service_id = table.Column<long>(type: "bigint", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    alert_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    triggered_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                    is_acknowledged = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    resolved_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    message = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerts", x => x.alert_id);
                    table.ForeignKey(
                        name: "FK_alerts_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_alerts_users_user_guid",
                        column: x => x.user_guid,
                        principalTable: "users",
                        principalColumn: "user_guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_configs",
                columns: table => new
                {
                    notification_config_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    rule_id = table.Column<long>(type: "bigint", nullable: false),
                    uri = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_configs", x => x.notification_config_guid);
                    table.ForeignKey(
                        name: "FK_notification_configs_alert_rules_rule_id",
                        column: x => x.rule_id,
                        principalTable: "alert_rules",
                        principalColumn: "rule_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alert_rules_service_id",
                table: "alert_rules",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_is_resolved",
                table: "alerts",
                column: "is_resolved");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_service_id",
                table: "alerts",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_triggered_at",
                table: "alerts",
                column: "triggered_at");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_user_guid",
                table: "alerts",
                column: "user_guid");

            migrationBuilder.CreateIndex(
                name: "IX_health_checks_checked_at",
                table: "health_checks",
                column: "checked_at");

            migrationBuilder.CreateIndex(
                name: "IX_health_checks_service_id",
                table: "health_checks",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_health_checks_status",
                table: "health_checks",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_notification_configs_rule_id",
                table: "notification_configs",
                column: "rule_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_dependencies_depends_on_service_id",
                table: "service_dependencies",
                column: "depends_on_service_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_dependencies_service_id",
                table: "service_dependencies",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_services_last_seen_at",
                table: "services",
                column: "last_seen_at");

            migrationBuilder.CreateIndex(
                name: "IX_services_service_name",
                table: "services",
                column: "service_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerts");

            migrationBuilder.DropTable(
                name: "health_checks");

            migrationBuilder.DropTable(
                name: "notification_configs");

            migrationBuilder.DropTable(
                name: "service_dependencies");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "alert_rules");

            migrationBuilder.DropTable(
                name: "services");
        }
    }
}
