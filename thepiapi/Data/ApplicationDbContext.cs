using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using thepiapi.Models;

namespace thepiapi.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<Budget> Budgets { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<SavingsGoal> SavingsGoals { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This is just a fallback. Program.cs will usually provide the connection.
            optionsBuilder.UseSqlite("Data Source=FinanceApp.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(e => e.Balance).HasDefaultValue(0.0);
            entity.Property(e => e.Color).HasDefaultValue("#3B82F6");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");
            entity.Property(e => e.Currency).HasDefaultValue("USD");
            entity.Property(e => e.Icon).HasDefaultValue("bank");
            entity.Property(e => e.IncludeInTotal)
                .HasDefaultValue(true)
                .HasColumnType("BOOLEAN");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnType("BOOLEAN");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");

            entity.HasOne(d => d.User).WithMany(p => p.Accounts).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnType("BOOLEAN");
            entity.Property(e => e.NextDueDate).HasColumnType("DATE");

            entity.HasOne(d => d.Category).WithMany(p => p.Bills)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.Bills).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnType("BOOLEAN");
            entity.Property(e => e.StartDate).HasColumnType("DATE");

            entity.HasOne(d => d.Category).WithMany(p => p.Budgets).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.User).WithMany(p => p.Budgets).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Color).HasDefaultValue("#6B7280");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");
            entity.Property(e => e.Icon).HasDefaultValue("tag");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnType("BOOLEAN");
            entity.Property(e => e.IsEssential)
                .HasDefaultValue(false)
                .HasColumnType("BOOLEAN");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory).HasForeignKey(d => d.ParentCategoryId);

            entity.HasOne(d => d.User).WithMany(p => p.Categories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SavingsGoal>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");
            entity.Property(e => e.CurrentAmount).HasDefaultValue(0.0);
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false)
                .HasColumnType("BOOLEAN");
            entity.Property(e => e.TargetDate).HasColumnType("DATE");

            entity.HasOne(d => d.User).WithMany(p => p.SavingsGoals).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");
            entity.Property(e => e.TransactionDate).HasColumnType("DATETIME");

            entity.HasOne(d => d.Account).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Category).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.Transactions).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.HasIndex(e => e.Username, "IX_Users_Username").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");
            entity.Property(e => e.Currency).HasDefaultValue("USD");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnType("BOOLEAN");
            entity.Property(e => e.LastLogin).HasColumnType("DATETIME");
            entity.Property(e => e.MonthlyIncome).HasDefaultValue(0.0);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
