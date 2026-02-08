using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedCrossManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingEnrollments_Certifications_CertificationId",
                table: "TrainingEnrollments");

            migrationBuilder.RenameColumn(
                name: "CertificationId",
                table: "TrainingEnrollments",
                newName: "CertificateId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingEnrollments_CertificationId",
                table: "TrainingEnrollments",
                newName: "IX_TrainingEnrollments_CertificateId");

            migrationBuilder.RenameColumn(
                name: "GuardianPhone",
                table: "CommunicationRecipients",
                newName: "RecipientPhone");

            migrationBuilder.RenameColumn(
                name: "GuardianEmail",
                table: "CommunicationRecipients",
                newName: "RecipientEmail");

            migrationBuilder.RenameColumn(
                name: "Channel",
                table: "CommunicationMessages",
                newName: "Channels");

            migrationBuilder.AlterColumn<Guid>(
                name: "VolunteerId",
                table: "CommunicationRecipients",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationRecipients_VolunteerId",
                table: "CommunicationRecipients",
                column: "VolunteerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationRecipients_Volunteers_VolunteerId",
                table: "CommunicationRecipients",
                column: "VolunteerId",
                principalTable: "Volunteers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingEnrollments_Certifications_CertificateId",
                table: "TrainingEnrollments",
                column: "CertificateId",
                principalTable: "Certifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunicationRecipients_Volunteers_VolunteerId",
                table: "CommunicationRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingEnrollments_Certifications_CertificateId",
                table: "TrainingEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_CommunicationRecipients_VolunteerId",
                table: "CommunicationRecipients");

            migrationBuilder.RenameColumn(
                name: "CertificateId",
                table: "TrainingEnrollments",
                newName: "CertificationId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingEnrollments_CertificateId",
                table: "TrainingEnrollments",
                newName: "IX_TrainingEnrollments_CertificationId");

            migrationBuilder.RenameColumn(
                name: "RecipientPhone",
                table: "CommunicationRecipients",
                newName: "GuardianPhone");

            migrationBuilder.RenameColumn(
                name: "RecipientEmail",
                table: "CommunicationRecipients",
                newName: "GuardianEmail");

            migrationBuilder.RenameColumn(
                name: "Channels",
                table: "CommunicationMessages",
                newName: "Channel");

            migrationBuilder.AlterColumn<Guid>(
                name: "VolunteerId",
                table: "CommunicationRecipients",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingEnrollments_Certifications_CertificationId",
                table: "TrainingEnrollments",
                column: "CertificationId",
                principalTable: "Certifications",
                principalColumn: "Id");
        }
    }
}
