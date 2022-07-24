using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Models
{
    public partial class OnlineStoreContext : DbContext
    {
        public OnlineStoreContext()
        {
        }

        public OnlineStoreContext(DbContextOptions<OnlineStoreContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Ethnicity> Ethnicities { get; set; } = null!;
        public virtual DbSet<Gender> Genders { get; set; } = null!;
        public virtual DbSet<Item> Items { get; set; } = null!;
        public virtual DbSet<ItemCharacteristic> ItemCharacteristics { get; set; } = null!;
        public virtual DbSet<Permission> Permissions { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=\"Online Store\";Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(128)
                    .IsUnicode(false)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Ethnicity>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Ethnicity1)
                    .IsUnicode(false)
                    .HasColumnName("ethnicity");
            });

            modelBuilder.Entity<Gender>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Gender1)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("gender");
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CharacteristicFk).HasColumnName("characteristic_fk");

                entity.Property(e => e.Name)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.PictureLocation)
                    .IsUnicode(false)
                    .HasColumnName("pictureLocation");

                entity.Property(e => e.Price).HasColumnName("price");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.Supplier)
                    .IsUnicode(false)
                    .HasColumnName("supplier");

                entity.HasOne(d => d.CharacteristicFkNavigation)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.CharacteristicFk)
                    .HasConstraintName("FK__Items__character__38996AB5");
            });

            modelBuilder.Entity<ItemCharacteristic>(entity =>
            {
                entity.ToTable("ItemCharacteristic");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.Color)
                    .HasMaxLength(128)
                    .IsUnicode(false)
                    .HasColumnName("color");

                entity.Property(e => e.Description)
                    .IsUnicode(false)
                    .HasColumnName("description");

                entity.Property(e => e.Height)
                    .IsUnicode(false)
                    .HasColumnName("height");

                entity.Property(e => e.Notes)
                    .IsUnicode(false)
                    .HasColumnName("notes");

                entity.Property(e => e.Width)
                    .IsUnicode(false)
                    .HasColumnName("width");

                entity.HasOne(d => d.CategoryNavigation)
                    .WithMany(p => p.ItemCharacteristics)
                    .HasForeignKey(d => d.Category)
                    .HasConstraintName("FK__ItemChara__categ__4AB81AF0");
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Permission1)
                    .IsUnicode(false)
                    .HasColumnName("permission");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.PermissionsFk).HasColumnName("permissions_fk");

                entity.Property(e => e.RoleName)
                    .IsUnicode(false)
                    .HasColumnName("role_name");

                entity.HasOne(d => d.PermissionsFkNavigation)
                    .WithMany(p => p.Roles)
                    .HasForeignKey(d => d.PermissionsFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Roles__permissio__73BA3083");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address).HasColumnName("address");

                entity.Property(e => e.Age).HasColumnName("age");

                entity.Property(e => e.Email).HasColumnName("email");

                entity.Property(e => e.EmailVerified)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .HasColumnName("emailVerified");

                entity.Property(e => e.Ethnicity).HasColumnName("ethnicity");

                entity.Property(e => e.FirstName).HasColumnName("firstName");

                entity.Property(e => e.LastLogin).HasColumnName("lastLogin");

                entity.Property(e => e.LastName).HasColumnName("lastName");

                entity.Property(e => e.Password).HasColumnName("password");

                entity.Property(e => e.Sex).HasColumnName("sex");

                entity.HasOne(d => d.EthnicityNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.Ethnicity)
                    .HasConstraintName("FK__user__ethnicity__03F0984C");

                entity.HasOne(d => d.RoleNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.Role)
                    .HasConstraintName("FK__user__Role__74AE54BC");

                entity.HasOne(d => d.SexNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.Sex)
                    .HasConstraintName("FK__user__sex__75A278F5");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
