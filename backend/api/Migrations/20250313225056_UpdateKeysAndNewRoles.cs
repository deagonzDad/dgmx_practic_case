using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKeysAndNewRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Rooms_RoomId",
                table: "Reservations"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_Rooms", table: "Rooms");

            migrationBuilder.DropIndex(name: "IX_Rooms_RoomNumber", table: "Rooms");

            migrationBuilder
                .AlterColumn<int>(
                    name: "RoomNumber",
                    table: "Rooms",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder
                .AddColumn<int>(
                    name: "Id",
                    table: "Rooms",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0
                )
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(name: "PK_Rooms", table: "Rooms", column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Id",
                table: "Rooms",
                column: "Id",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Rooms_RoomId",
                table: "Reservations",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Rooms_RoomId",
                table: "Reservations"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_Rooms", table: "Rooms");

            migrationBuilder.DropIndex(name: "IX_Rooms_Id", table: "Rooms");

            migrationBuilder.DropColumn(name: "Id", table: "Rooms");

            migrationBuilder
                .AlterColumn<int>(
                    name: "RoomNumber",
                    table: "Rooms",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(name: "PK_Rooms", table: "Rooms", column: "RoomNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomNumber",
                table: "Rooms",
                column: "RoomNumber",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Rooms_RoomId",
                table: "Reservations",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "RoomNumber",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
