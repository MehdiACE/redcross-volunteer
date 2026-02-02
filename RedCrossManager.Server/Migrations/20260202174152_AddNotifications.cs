using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedCrossManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingEnrollments_Certifications_CertificateId",
                table: "TrainingEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_Trainings_StartAt_Published",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "Prerequisites",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "Published",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "AttendanceStatus",
                table: "TrainingEnrollments");

            migrationBuilder.DropColumn(
                name: "CompletionStatus",
                table: "TrainingEnrollments");

            migrationBuilder.DropColumn(
                name: "EnrollmentStatus",
                table: "TrainingEnrollments");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "TrainingEnrollments");

            migrationBuilder.RenameColumn(
                name: "StartAt",
                table: "Trainings",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Trainings",
                newName: "LocationName");

            migrationBuilder.RenameColumn(
                name: "EndAt",
                table: "Trainings",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Trainings",
                newName: "CreatedByCoordinatorId");

            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "Trainings",
                newName: "MaxEnrollment");

            migrationBuilder.RenameColumn(
                name: "CertificateId",
                table: "TrainingEnrollments",
                newName: "VolunteerId1");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingEnrollments_CertificateId",
                table: "TrainingEnrollments",
                newName: "IX_TrainingEnrollments_VolunteerId1");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Trainings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Trainings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Trainings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CertificateIssuedAt",
                table: "TrainingEnrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateNumber",
                table: "TrainingEnrollments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CertificationId",
                table: "TrainingEnrollments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TrainingEnrollments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TrainingEnrollments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VolunteerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Volunteers_VolunteerId",
                        column: x => x.VolunteerId,
                        principalTable: "Volunteers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_StartDate_Status",
                table: "Trainings",
                columns: new[] { "StartDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingEnrollments_CertificationId",
                table: "TrainingEnrollments",
                column: "CertificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_VolunteerId",
                table: "Notifications",
                column: "VolunteerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingEnrollments_Certifications_CertificationId",
                table: "TrainingEnrollments",
                column: "CertificationId",
                principalTable: "Certifications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingEnrollments_Volunteers_VolunteerId1",
                table: "TrainingEnrollments",
                column: "VolunteerId1",
                principalTable: "Volunteers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingEnrollments_Certifications_CertificationId",
                table: "TrainingEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingEnrollments_Volunteers_VolunteerId1",
                table: "TrainingEnrollments");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Trainings_StartDate_Status",
                table: "Trainings");

            migrationBuilder.DropIndex(
                name: "IX_TrainingEnrollments_CertificationId",
                table: "TrainingEnrollments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Trainings");

            migrationBuilder.DropColumn(
                name: "CertificateIssuedAt",
                table: "TrainingEnrollments");

            migrationBuilder.DropColumn(
                name: "CertificateNumber",
                table: "TrainingEnrollments");

            migrationBuilder.DropColumn(
                name: "CertificationId",
                table: "TrainingEnrollments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TrainingEnrollments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TrainingEnrollments");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Trainings",
                newName: "StartAt");

            migrationBuilder.RenameColumn(
                name: "MaxEnrollment",
                table: "Trainings",
                newName: "Capacity");

            migrationBuilder.RenameColumn(
                name: "LocationName",
                table: "Trainings",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Trainings",
                newName: "EndAt");

            migrationBuilder.RenameColumn(
                name: "CreatedByCoordinatorId",
                table: "Trainings",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "VolunteerId1",
                table: "TrainingEnrollments",
                newName: "CertificateId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingEnrollments_VolunteerId1",
                table: "TrainingEnrollments",
                newName: "IX_TrainingEnrollments_CertificateId");

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "Trainings",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Prerequisites",
                table: "Trainings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Published",
                table: "Trainings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AttendanceStatus",
                table: "TrainingEnrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompletionStatus",
                table: "TrainingEnrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EnrollmentStatus",
                table: "TrainingEnrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "TrainingEnrollments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_StartAt_Published",
                table: "Trainings",
                columns: new[] { "StartAt", "Published" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingEnrollments_Certifications_CertificateId",
                table: "TrainingEnrollments",
                column: "CertificateId",
                principalTable: "Certifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
