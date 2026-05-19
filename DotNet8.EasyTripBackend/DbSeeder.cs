using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotNet8.EasyTripBackendApi.DbService.Models;

namespace DotNet8.EasyTripBackend
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            try
            {
                // Ensure connection can open
                await context.Database.OpenConnectionAsync();

                // Setup schema for bookings and booking_details
                using (var cmd = context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = @"
                        -- Add updated_at to bookings if missing
                        ALTER TABLE bookings ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE NULL;

                        -- Make item_id nullable to support details mapping
                        ALTER TABLE bookings ALTER COLUMN item_id DROP NOT NULL;

                        -- Create booking_details table if not exists
                        CREATE TABLE IF NOT EXISTS booking_details (
                            id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                            booking_id BIGINT NOT NULL,
                            bus_id BIGINT NULL,
                            hotel_room_id BIGINT NULL,
                            package_id BIGINT NULL,
                            selected_seats VARCHAR(255) NULL,
                            quantity INT NOT NULL,
                            travel_date DATE NOT NULL,
                            end_date DATE NULL,
                            CONSTRAINT fk_booking_details_booking FOREIGN KEY (booking_id) REFERENCES bookings(id) ON DELETE CASCADE,
                            CONSTRAINT fk_booking_details_bus FOREIGN KEY (bus_id) REFERENCES buses(id) ON DELETE SET NULL,
                            CONSTRAINT fk_booking_details_hotel_room FOREIGN KEY (hotel_room_id) REFERENCES hotel_rooms(id) ON DELETE SET NULL,
                            CONSTRAINT fk_booking_details_package FOREIGN KEY (package_id) REFERENCES travel_packages(id) ON DELETE SET NULL
                        );
                    ";
                    await cmd.ExecuteNonQueryAsync();
                }


                // 1. Rename existing shorthand values in place to preserve foreign key links
                var roomTypes = await context.RoomTypes.ToListAsync();
                
                var single = roomTypes.FirstOrDefault(rt => rt.TypeName == "Single");
                if (single != null) single.TypeName = "Single Room";

                var doubleRoom = roomTypes.FirstOrDefault(rt => rt.TypeName == "Double");
                if (doubleRoom != null) doubleRoom.TypeName = "Double Room";

                var suite = roomTypes.FirstOrDefault(rt => rt.TypeName == "Suite");
                if (suite != null) suite.TypeName = "Suite Room";

                var family = roomTypes.FirstOrDefault(rt => rt.TypeName == "Family");
                if (family != null) family.TypeName = "Family Room";

                // Save renaming updates
                await context.SaveChangesAsync();

                // 2. Insert any missing default room types
                var targetTypes = new[] { "Single Room", "Double Room", "Suite Room", "Family Room" };
                
                foreach (var typeName in targetTypes)
                {
                    var exists = await context.RoomTypes.AnyAsync(rt => rt.TypeName == typeName);
                    if (!exists)
                    {
                        context.RoomTypes.Add(new RoomType { TypeName = typeName });
                    }
                }

                await context.SaveChangesAsync();
                Console.WriteLine("Database room types lookup seeded successfully with exact naming standards!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error seeding room types lookup table: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }
    }
}
