using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BusinessProgressSoft.Models;

public partial class BusinessProgressSoftContext : DbContext
{
    public BusinessProgressSoftContext()
    {
    }

    public BusinessProgressSoftContext(DbContextOptions<BusinessProgressSoftContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bcard> Bcards { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-V7VKI3K;Database=BusinessProgressSoft;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bcard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BCard__3213E83F591C39FC");

            entity.ToTable("BCard");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.Birth)
                .HasColumnType("date")
                .HasColumnName("birth");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("gender");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Photo)
                .IsUnicode(false)
                .HasColumnName("photo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
