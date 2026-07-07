using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enquiry.api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailSubjectMessageToEnquiries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "EnquiryMasters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "EnquiryMasters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "EnquiryMasters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "EnquiryMasters");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "EnquiryMasters");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "EnquiryMasters");
        }
    }
}
