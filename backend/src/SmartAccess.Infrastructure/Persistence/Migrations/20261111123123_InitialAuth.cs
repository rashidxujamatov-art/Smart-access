using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAccess.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ts = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_login = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    device = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    object_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    object_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    old_value = table.Column<string>(type: "jsonb", nullable: true),
                    new_value = table.Column<string>(type: "jsonb", nullable: true),
                    result = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    value = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    login = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    totp_secret_enc = table.Column<byte[]>(type: "bytea", nullable: true),
                    pending_totp_secret_enc = table.Column<byte[]>(type: "bytea", nullable: true),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failed_attempts = table.Column<int>(type: "integer", nullable: false),
                    locked_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    refresh_token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    device = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    remember_me = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_id_category_ts",
                table: "audit_logs",
                columns: new[] { "tenant_id", "category", "ts" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_id_ts",
                table: "audit_logs",
                columns: new[] { "tenant_id", "ts" });

            migrationBuilder.CreateIndex(
                name: "ix_sessions_refresh_token_hash",
                table: "sessions",
                column: "refresh_token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sessions_tenant_id_user_id",
                table: "sessions",
                columns: new[] { "tenant_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_sessions_user_id",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_settings_tenant_id_key",
                table: "settings",
                columns: new[] { "tenant_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_code",
                table: "tenants",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_tenant_id_login",
                table: "users",
                columns: new[] { "tenant_id", "login" },
                unique: true,
                filter: "deleted_at IS NULL");

            // BR-AUD-03: audit log faqat qo'shiladi — UPDATE, DELETE va TRUNCATE baza darajasida taqiqlangan.
            migrationBuilder.Sql("""
                CREATE FUNCTION audit_logs_append_only() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    RAISE EXCEPTION 'audit_logs is append-only (BR-AUD-03)' USING ERRCODE = 'insufficient_privilege';
                END;
                $$;
                CREATE TRIGGER audit_logs_no_update_delete
                    BEFORE UPDATE OR DELETE ON audit_logs
                    FOR EACH ROW EXECUTE FUNCTION audit_logs_append_only();
                CREATE TRIGGER audit_logs_no_truncate
                    BEFORE TRUNCATE ON audit_logs
                    FOR EACH STATEMENT EXECUTE FUNCTION audit_logs_append_only();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS audit_logs_no_truncate ON audit_logs;
                DROP TRIGGER IF EXISTS audit_logs_no_update_delete ON audit_logs;
                DROP FUNCTION IF EXISTS audit_logs_append_only();
                """);

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
