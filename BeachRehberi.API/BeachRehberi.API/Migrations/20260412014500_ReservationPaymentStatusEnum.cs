using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachRehberi.API.Migrations
{
    public partial class ReservationPaymentStatusEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Reservations"
                ALTER COLUMN "PaymentStatus" TYPE integer
                USING CASE
                    WHEN "PaymentStatus" = 'Paid' THEN 1
                    WHEN "PaymentStatus" = 'Failed' THEN 2
                    WHEN "PaymentStatus" = 'Refunded' THEN 3
                    ELSE 0
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Reservations"
                ALTER COLUMN "PaymentStatus" TYPE character varying(50)
                USING CASE
                    WHEN "PaymentStatus" = 1 THEN 'Paid'
                    WHEN "PaymentStatus" = 2 THEN 'Failed'
                    WHEN "PaymentStatus" = 3 THEN 'Refunded'
                    ELSE 'Pending'
                END;
                """);
        }
    }
}
