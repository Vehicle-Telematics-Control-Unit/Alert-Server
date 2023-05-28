using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Alert_Server.Models
{
    public partial class TCUContext : DbContext
    {
        public TCUContext()
        {
        }

        public TCUContext(DbContextOptions<TCUContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Alert> Alerts { get; set; } = null!;
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; } = null!;
        public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; } = null!;
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; } = null!;
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; } = null!;
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; } = null!;
        public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; } = null!;
        public virtual DbSet<ConnectionRequest> ConnectionRequests { get; set; } = null!;
        public virtual DbSet<ContactMethod> ContactMethods { get; set; } = null!;
        public virtual DbSet<Device> Devices { get; set; } = null!;
        public virtual DbSet<DevicesTcu> DevicesTcus { get; set; } = null!;
        public virtual DbSet<LockRequest> LockRequests { get; set; } = null!;
        public virtual DbSet<ObdCode> ObdCodes { get; set; } = null!;
        public virtual DbSet<ObdFaultAreaCode> ObdFaultAreaCodes { get; set; } = null!;
        public virtual DbSet<ObdSubSystemCode> ObdSubSystemCodes { get; set; } = null!;
        public virtual DbSet<Otptoken> Otptokens { get; set; } = null!;
        public virtual DbSet<RequestStatus> RequestStatuses { get; set; } = null!;
        public virtual DbSet<Tcu> Tcus { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("UserID=postgres;Password=postgres;Server=vehicleplus.cloud;Port=5435;Database=TCU; Integrated Security=true;Pooling=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasKey(e => new { e.LogTimeStamp, e.ObdCode, e.TcuId })
                    .HasName("Alerts_pkey");

                entity.Property(e => e.ObdCode)
                    .HasMaxLength(5)
                    .IsFixedLength();

                entity.HasOne(d => d.ObdCodeNavigation)
                    .WithMany(p => p.Alerts)
                    .HasForeignKey(d => d.ObdCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Alert_ObdCodes");

                entity.HasOne(d => d.Tcu)
                    .WithMany(p => p.Alerts)
                    .HasForeignKey(d => d.TcuId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Alert_TCU");
            });

            modelBuilder.Entity<AspNetRole>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetRoleClaim>(entity =>
            {
                entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AspNetUser>(entity =>
            {
                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(256);

                entity.HasMany(d => d.Roles)
                    .WithMany(p => p.Users)
                    .UsingEntity<Dictionary<string, object>>(
                        "AspNetUserRole",
                        l => l.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                        r => r.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                        j =>
                        {
                            j.HasKey("UserId", "RoleId");

                            j.ToTable("AspNetUserRoles");

                            j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                        });
            });

            modelBuilder.Entity<AspNetUserClaim>(entity =>
            {
                entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogin>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserToken>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserTokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<ConnectionRequest>(entity =>
            {
                entity.HasKey(e => new { e.TcuId, e.DeviceId, e.CreationTimeStamp })
                    .HasName("ConnectionRequests_pkey");

                entity.Property(e => e.DeviceId).HasMaxLength(150);

                entity.Property(e => e.CreationTimeStamp).HasDefaultValueSql("now()");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.ConnectionRequests)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ConnectionRequest_Device");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.ConnectionRequests)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ConnectionRequest_RequestStatuses");

                entity.HasOne(d => d.Tcu)
                    .WithMany(p => p.ConnectionRequests)
                    .HasForeignKey(d => d.TcuId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ConnectionRequest_TCU");
            });

            modelBuilder.Entity<ContactMethod>(entity =>
            {
                entity.HasKey(e => new { e.Type, e.Value, e.UserId })
                    .HasName("Contact_methods_pkey");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.IsPrimary).HasColumnName("isPrimary");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ContactMethods)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("ContactMethods_AspNetUsers");
            });

            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("Device");

                entity.Property(e => e.DeviceId).HasMaxLength(150);

                entity.Property(e => e.IpAddress).HasMaxLength(15);

                entity.Property(e => e.LastLoginTime).HasDefaultValueSql("'2023-05-23 19:50:17.208823+00'::timestamp with time zone");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Devices)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Devices_AspNetUsers");
            });

            modelBuilder.Entity<DevicesTcu>(entity =>
            {
                entity.HasKey(e => new { e.DeviceId, e.TcuId })
                    .HasName("DevicesTcu_pkey");

                entity.ToTable("DevicesTcu");

                entity.Property(e => e.DeviceId).HasMaxLength(150);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("isActive")
                    .HasDefaultValueSql("true");

                entity.Property(e => e.IsPrimary).HasColumnName("isPrimary");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.DevicesTcus)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Device_fkey");

                entity.HasOne(d => d.Tcu)
                    .WithMany(p => p.DevicesTcus)
                    .HasForeignKey(d => d.TcuId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TCU_fkey");
            });

            modelBuilder.Entity<LockRequest>(entity =>
            {
                entity.HasKey(e => new { e.TcuId, e.DeviceId, e.CreationTimeStamp })
                    .HasName("LockRequests_pkey");

                entity.Property(e => e.DeviceId).HasMaxLength(150);

                entity.Property(e => e.CreationTimeStamp).HasDefaultValueSql("now()");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.LockRequests)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("LockRequest_Device");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.LockRequests)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("LockRequest_RequestStatuses");

                entity.HasOne(d => d.Tcu)
                    .WithMany(p => p.LockRequests)
                    .HasForeignKey(d => d.TcuId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("LockRequest_TCU");
            });

            modelBuilder.Entity<ObdCode>(entity =>
            {
                entity.HasKey(e => e.ObdCode1)
                    .HasName("ObdCodes_pkey");

                entity.Property(e => e.ObdCode1)
                    .HasMaxLength(5)
                    .HasColumnName("ObdCode")
                    .IsFixedLength();

                entity.Property(e => e.IsGeneric)
                    .IsRequired()
                    .HasColumnName("isGeneric")
                    .HasDefaultValueSql("true");
            });

            modelBuilder.Entity<ObdFaultAreaCode>(entity =>
            {
                entity.HasKey(e => new { e.AreaId, e.Description })
                    .HasName("ObdFaultAreaCodes_pkey");

                entity.Property(e => e.AreaId).HasMaxLength(1);
            });

            modelBuilder.Entity<ObdSubSystemCode>(entity =>
            {
                entity.HasKey(e => e.SubsystemId)
                    .HasName("ObdSubSystemCodes_pkey");

                entity.Property(e => e.SubsystemId)
                    .HasMaxLength(1)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<Otptoken>(entity =>
            {
                entity.ToTable("otptokens");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Token).HasColumnName("token");

                entity.Property(e => e.Userid).HasColumnName("userid");

                entity.Property(e => e.Verifiedat)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("verifiedat");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Otptokens)
                    .HasForeignKey(d => d.Userid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OTP_USERS");
            });

            modelBuilder.Entity<RequestStatus>(entity =>
            {
                entity.HasKey(e => e.StatusId)
                    .HasName("RequestStatuses_pkey");

                entity.Property(e => e.StatusId).ValueGeneratedNever();
            });

            modelBuilder.Entity<Tcu>(entity =>
            {
                entity.ToTable("TCU");

                entity.Property(e => e.ExpiresAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.IpAddress).HasColumnType("character varying");

                entity.Property(e => e.Mac).HasMaxLength(17);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Tcus)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TCU_AspNetUsers");
            });

            modelBuilder.HasSequence("otptokens_id_seq");

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
