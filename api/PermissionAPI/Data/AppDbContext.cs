using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PermissionAPI.Data.Entities;
using PermissionAPI.Models.SpResults;

namespace PermissionAPI.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPermissionResult> UserPermissionResults { get; set; } // Added DbSet for UserPermissionResult

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Groups__149AF36A439DE7CE");

            entity.HasIndex(e => e.GroupName, "UQ__Groups__6EFCD4349199E425").IsUnique();

            entity.Property(e => e.GroupName).HasMaxLength(100);

            entity.HasMany(d => d.Permissions).WithMany(p => p.Groups)
                .UsingEntity<Dictionary<string, object>>(
                    "GroupAndPermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_GAP_Permissions"),
                    l => l.HasOne<Group>().WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_GAP_Groups"),
                    j =>
                    {
                        j.HasKey("GroupId", "PermissionId");
                        j.ToTable("GroupAndPermissions");
                    });
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__EFA6FB2F0591DE94");

            entity.HasIndex(e => e.PermissionKey, "UQ__Permissi__8884ABD4515280CE").IsUnique();

            entity.Property(e => e.PermissionKey).HasMaxLength(100);
            entity.Property(e => e.PermissionName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C209623A1");

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534B2E51F7A").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasDefaultValue("");
            entity.Property(e => e.PasswordSalt).HasDefaultValue("");
            entity.Property(e => e.UserName).HasMaxLength(100);

            entity.HasMany(d => d.Groups).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserGroup",
                    r => r.HasOne<Group>().WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UG_Groups"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UG_Users"),
                    j =>
                    {
                        j.HasKey("UserId", "GroupId");
                        j.ToTable("UserGroups");
                    });

            entity.HasMany(d => d.Permissions).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserDirectPermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UDP_Permissions"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UDP_Users"),
                    j =>
                    {
                        j.HasKey("UserId", "PermissionId");
                        j.ToTable("UserDirectPermissions");
                    });
        });

        modelBuilder.Entity<UserPermissionResult>().HasNoKey().ToView(null); // Mapping for UserPermissionResult

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

// Ensure this class exists and matches the columns returned by sp_GetUserPermissions
public class UserPermissionResult
{
    public string UserName { get; set; }
    public string PermissionKey { get; set; }
    public string PermissionName { get; set; }
    public string? GroupName { get; set; }
}
