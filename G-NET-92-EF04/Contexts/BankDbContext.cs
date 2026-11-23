using G_NET_92_EF04.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace G_NET_92_EF04.Contexts
{
    internal class BankDbContext : DbContext
    {
        public BankDbContext(DbContextOptions<BankDbContext> options)
    : base(options)
        {
        }

        public DbSet<Branch> Branches { get; set; }
            public DbSet<Manager> Managers { get; set; }
            public DbSet<Customer> Customers { get; set; }
            public DbSet<Account> Accounts { get; set; }
            public DbSet<CustomerAccount> CustomerAccounts { get; set; }
            public DbSet<Transaction> Transactions { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Branch
                modelBuilder.Entity<Branch>()
                    .HasKey(b => b.Code);

                modelBuilder.Entity<Branch>()
                    .Property(b => b.Code)
                    .HasMaxLength(10);

                // Branch - Manager (One to One)
                modelBuilder.Entity<Branch>()
                    .HasOne(b => b.Manager)
                    .WithOne(m => m.Branch)
                    .HasForeignKey<Manager>(m => m.BranchCode)
                    .OnDelete(DeleteBehavior.Restrict);

                // Branch - Account (One to Many)
                modelBuilder.Entity<Branch>()
                    .HasMany(b => b.Accounts)
                    .WithOne(a => a.Branch)
                    .HasForeignKey(a => a.BranchCode)
                    .OnDelete(DeleteBehavior.Restrict);

                // Customer
                modelBuilder.Entity<Customer>()
                    .HasIndex(c => c.NationalId)
                    .IsUnique();

                modelBuilder.Entity<Customer>()
                    .Property(c => c.CustomerType)
                    .HasConversion<string>();

                // Account
                modelBuilder.Entity<Account>()
                    .HasKey(a => a.AccountNumber);

                modelBuilder.Entity<Account>()
                    .Property(a => a.CurrentBalance)
                    .HasPrecision(18, 2);

                // Customer - Account (Many to Many)
                modelBuilder.Entity<CustomerAccount>()
                    .HasKey(ca => new { ca.CustomerId, ca.AccountNumber });

                modelBuilder.Entity<CustomerAccount>()
                    .Property(ca => ca.OwnershipType)
                    .HasConversion<string>();

                modelBuilder.Entity<CustomerAccount>()
                    .Property(ca => ca.AccountStatus)
                    .HasConversion<string>();

                modelBuilder.Entity<CustomerAccount>()
                    .HasOne(ca => ca.Customer)
                    .WithMany(c => c.CustomerAccounts)
                    .HasForeignKey(ca => ca.CustomerId);

                modelBuilder.Entity<CustomerAccount>()
                    .HasOne(ca => ca.Account)
                    .WithMany(a => a.CustomerAccounts)
                    .HasForeignKey(ca => ca.AccountNumber);

                // Account - Transaction (One to Many)
                modelBuilder.Entity<Account>()
                    .HasMany(a => a.Transactions)
                    .WithOne(t => t.Account)
                    .HasForeignKey(t => t.AccountNumber);

                modelBuilder.Entity<Transaction>()
                    .HasKey(t => t.TransactionNumber);

                modelBuilder.Entity<Transaction>()
                    .Property(t => t.Amount)
                    .HasPrecision(18, 2);

                // Seed Branches
                modelBuilder.Entity<Branch>().HasData(
                    new Branch
                    {
                        Code = "B001",
                        Name = "Cairo Branch",
                        Address = "Nasr City, Cairo",
                        PhoneNumber = "01000000001"
                    },
                    new Branch
                    {
                        Code = "B002",
                        Name = "Giza Branch",
                        Address = "Dokki, Giza",
                        PhoneNumber = "01000000002"
                    }
                );

                // Seed Managers
                modelBuilder.Entity<Manager>().HasData(
                    new Manager
                    {
                        Id = 1,
                        FullName = "Ahmed Mohamed",
                        Email = "ahmed@bank.com",
                        PhoneNumber = "01000000003",
                        HireDate = new DateTime(2024, 1, 1),
                        BranchCode = "B001"
                    },
                    new Manager
                    {
                        Id = 2,
                        FullName = "Mohamed Ali",
                        Email = "mohamed@bank.com",
                        PhoneNumber = "01000000004",
                        HireDate = new DateTime(2024, 2, 1),
                        BranchCode = "B002"
                    }
                );
            }
        }
    }


