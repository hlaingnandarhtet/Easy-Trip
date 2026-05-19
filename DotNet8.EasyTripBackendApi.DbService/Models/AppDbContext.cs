using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DotNet8.EasyTripBackendApi.DbService.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingDetail> BookingDetails { get; set; }

    public virtual DbSet<Bus> Buses { get; set; }

    public virtual DbSet<BusSeat> BusSeats { get; set; }

    public virtual DbSet<Hotel> Hotels { get; set; }

    public virtual DbSet<HotelRoom> HotelRooms { get; set; }

    public virtual DbSet<RoomType> RoomTypes { get; set; }

    public virtual DbSet<TravelPackage> TravelPackages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=db.zjjmggyrlhgbdyormcup.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=2612000@#$!!$;Ssl Mode=Require;Trust Server Certificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
            .HasPostgresEnum("auth", "oauth_authorization_status", new[] { "pending", "approved", "denied", "expired" })
            .HasPostgresEnum("auth", "oauth_client_type", new[] { "public", "confidential" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "oauth_response_type", new[] { "code" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS", "VECTOR" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bookings_pkey");

            entity.ToTable("bookings");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.BookingDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("booking_date");
            entity.Property(e => e.BookingStatus)
                .HasDefaultValue(0)
                .HasColumnName("booking_status");
            entity.Property(e => e.BookingType)
                .HasMaxLength(50)
                .HasColumnName("booking_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.ItemId).HasColumnName("item_id").IsRequired(false);
            entity.Property(e => e.PaymentStatus)
                .HasDefaultValue(0)
                .HasColumnName("payment_status");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.TravelDate).HasColumnName("travel_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("bookings_user_id_fkey");
        });

        modelBuilder.Entity<BookingDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("booking_details_pkey");

            entity.ToTable("booking_details");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.BusId).HasColumnName("bus_id");
            entity.Property(e => e.HotelRoomId).HasColumnName("hotel_room_id");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            
            entity.Property(e => e.SelectedSeats)
                .HasMaxLength(255)
                .HasColumnName("selected_seats");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TravelDate).HasColumnName("travel_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("fk_booking_details_booking");

            entity.HasOne(d => d.Bus).WithMany()
                .HasForeignKey(d => d.BusId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_booking_details_bus");

            entity.HasOne(d => d.HotelRoom).WithMany()
                .HasForeignKey(d => d.HotelRoomId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_booking_details_hotel_room");

            entity.HasOne(d => d.Package).WithMany()
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_booking_details_package");
        });


        modelBuilder.Entity<Bus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("buses_pkey");

            entity.ToTable("buses");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Arrival)
                .HasMaxLength(100)
                .HasColumnName("arrival");
            entity.Property(e => e.BusClass)
                .HasMaxLength(50)
                .HasColumnName("bus_class");
            entity.Property(e => e.BusName)
                .HasMaxLength(100)
                .HasColumnName("bus_name");
            entity.Property(e => e.BusNumber)
                .HasMaxLength(50)
                .HasColumnName("bus_number");
            entity.Property(e => e.BusStatus)
                .HasDefaultValue(0)
                .HasColumnName("bus_status");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Departure)
                .HasMaxLength(100)
                .HasColumnName("departure");
            entity.Property(e => e.DriverName)
                .HasMaxLength(100)
                .HasColumnName("driver_name");
            entity.Property(e => e.EndPoint)
                .HasMaxLength(255)
                .HasColumnName("end_point");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.StartPoint)
                .HasMaxLength(255)
                .HasColumnName("start_point");
            entity.Property(e => e.TimeSlot)
                .HasMaxLength(50)
                .HasColumnName("time_slot");
            entity.Property(e => e.TotalSeats).HasColumnName("total_seats");
            entity.Property(e => e.TripType)
                .HasMaxLength(50)
                .HasColumnName("trip_type");
        });

        modelBuilder.Entity<BusSeat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bus_seats_pkey");

            entity.ToTable("bus_seats");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.BusId).HasColumnName("bus_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsBooked)
                .HasDefaultValue(false)
                .HasColumnName("is_booked");
            entity.Property(e => e.SeatNo)
                .HasMaxLength(10)
                .HasColumnName("seat_no");

            entity.HasOne(d => d.Bus).WithMany(p => p.BusSeats)
                .HasForeignKey(d => d.BusId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("bus_seats_bus_id_fkey");
        });

        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hotels_pkey");

            entity.ToTable("hotels");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.HotelName)
                .HasMaxLength(100)
                .HasColumnName("hotel_name");
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .HasColumnName("location");
        });

        modelBuilder.Entity<HotelRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hotel_rooms_pkey");

            entity.ToTable("hotel_rooms");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Amenities).HasColumnName("amenities");
            entity.Property(e => e.AvailableRooms).HasColumnName("available_rooms");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.HotelId).HasColumnName("hotel_id");
            entity.Property(e => e.HotelStatus)
                .HasDefaultValue(0)
                .HasColumnName("hotel_status");
            entity.Property(e => e.PricePerNight)
                .HasPrecision(10, 2)
                .HasColumnName("price_per_night");
            entity.Property(e => e.RoomTypeId).HasColumnName("room_type_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Hotel).WithMany(p => p.HotelRooms)
                .HasForeignKey(d => d.HotelId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("hotel_rooms_hotel_id_fkey");

            entity.HasOne(d => d.RoomType).WithMany(p => p.HotelRooms)
                .HasForeignKey(d => d.RoomTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("hotel_rooms_room_type_id_fkey");
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("room_types_pkey");

            entity.ToTable("room_types");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<TravelPackage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("travel_packages_pkey");

            entity.ToTable("travel_packages");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.BusId).HasColumnName("bus_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DiscountPercentage)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("discount_percentage");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.HotelId).HasColumnName("hotel_id");
            entity.Property(e => e.PackageName)
                .HasMaxLength(100)
                .HasColumnName("package_name");
            entity.Property(e => e.PackagePrice)
                .HasPrecision(10, 2)
                .HasColumnName("package_price");
            entity.Property(e => e.PackageStatus)
                .HasDefaultValue(0)
                .HasColumnName("package_status");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.TransferService)
                .HasDefaultValue(false)
                .HasColumnName("transfer_service");

            entity.HasOne(d => d.Bus).WithMany(p => p.TravelPackages)
                .HasForeignKey(d => d.BusId)
                .HasConstraintName("travel_packages_bus_id_fkey");

            entity.HasOne(d => d.Hotel).WithMany(p => p.TravelPackages)
                .HasForeignKey(d => d.HotelId)
                .HasConstraintName("travel_packages_hotel_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValueSql("'customer'::character varying")
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
