using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class update_student_teacher_entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "Students" WHERE "UserId" IS NULL) THEN
                        RAISE EXCEPTION 'Cannot require Student.UserId while Students contain rows without an identity user.';
                    END IF;

                    IF EXISTS (SELECT 1 FROM "Teachers" WHERE "UserId" IS NULL) THEN
                        RAISE EXCEPTION 'Cannot require Teacher.UserId while Teachers contain rows without an identity user.';
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                UPDATE "AspNetUsers" AS users
                SET "FirstName" = students."FirstName",
                    "LastName" = students."LastName"
                FROM "Students" AS students
                WHERE students."UserId" = users."Id";

                UPDATE "AspNetUsers" AS users
                SET "FirstName" = teachers."FirstName",
                    "LastName" = teachers."LastName"
                FROM "Teachers" AS teachers
                WHERE teachers."UserId" = users."Id";
                """);

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Students");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Teachers",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Students",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Teachers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Teachers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Teachers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Students",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Students",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Students",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Students" AS students
                SET "FirstName" = users."FirstName",
                    "LastName" = users."LastName"
                FROM "AspNetUsers" AS users
                WHERE students."UserId" = users."Id";

                UPDATE "Teachers" AS teachers
                SET "FirstName" = users."FirstName",
                    "LastName" = users."LastName"
                FROM "AspNetUsers" AS users
                WHERE teachers."UserId" = users."Id";
                """);
        }
    }
}
