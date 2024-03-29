﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LifeHelper.Server.Models.EF
{
    public partial class LifeHelperContext : DbContext
    {
        public LifeHelperContext()
        {
        }

        public LifeHelperContext(DbContextOptions<LifeHelperContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Accounting> Accountings { get; set; } = null!;
        public virtual DbSet<DeleteConfirm> DeleteConfirms { get; set; } = null!;
        public virtual DbSet<Memorandum> Memoranda { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Chinese_Taiwan_Stroke_CI_AS");

            modelBuilder.Entity<Accounting>(entity =>
            {
                entity.ToTable("Accounting");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccountDate).HasColumnType("datetime");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Event).HasMaxLength(50);
            });

            modelBuilder.Entity<DeleteConfirm>(entity =>
            {
                entity.ToTable("DeleteConfirm");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Deadline).HasColumnType("datetime");

                entity.Property(e => e.FeatureName).HasMaxLength(50);
            });

            modelBuilder.Entity<Memorandum>(entity =>
            {
                entity.ToTable("Memorandum");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Name).HasMaxLength(10);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
