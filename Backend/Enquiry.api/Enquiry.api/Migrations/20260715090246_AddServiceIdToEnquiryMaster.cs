using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enquiry.api.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceIdToEnquiryMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "serviceId",
                table: "EnquiryMasters",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryMasters_serviceId",
                table: "EnquiryMasters",
                column: "serviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnquiryMasters_Services_serviceId",
                table: "EnquiryMasters",
                column: "serviceId",
                principalTable: "Services",
                principalColumn: "serviceId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryMasters_Services_serviceId",
                table: "EnquiryMasters");

            migrationBuilder.DropIndex(
                name: "IX_EnquiryMasters_serviceId",
                table: "EnquiryMasters");

            migrationBuilder.DropColumn(
                name: "serviceId",
                table: "EnquiryMasters");
        }
    }
}
