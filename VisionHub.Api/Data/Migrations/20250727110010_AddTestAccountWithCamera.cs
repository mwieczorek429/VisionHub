using Microsoft.EntityFrameworkCore.Migrations;


namespace VisionHub.Api.Migrations
{
    public partial class AddTestAccountWithCamera : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM CameraEvents");
            migrationBuilder.Sql("DELETE FROM Cameras");
            migrationBuilder.Sql("DELETE FROM Users");

            // Hasło: "test" zahaszowane bcryptem
            var passwordHash = "$2a$11$TSGvgVdT7Lb.mgSd.4/SMuQf8YWI/Ezx1n2uJHyW1HOoia8AjqFhK";

            migrationBuilder.Sql($@"
                INSERT INTO Users (Login, PasswordHash)
                VALUES ('test', '{passwordHash}')
            ");

            migrationBuilder.Sql(@"
                DECLARE @UserId INT;
                SELECT @UserId = Id FROM Users WHERE Login = 'test';

                INSERT INTO Cameras (Name, Login, Password, Token, Url, AppUserId)
                VALUES 
                ('kamera-test-1', 'admin', 'admin', 'token', 'http://camera_app_1:8090', @UserId),
                ('kamera-test-2', 'admin', 'admin', 'token', 'http://camera_app_2:8091', @UserId)
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM CameraEvents");
            migrationBuilder.Sql("DELETE FROM Cameras WHERE Name IN ('kamera-test-1', 'kamera-test-2')");
            migrationBuilder.Sql("DELETE FROM Users WHERE Login = 'test'");
        }
    }
}
