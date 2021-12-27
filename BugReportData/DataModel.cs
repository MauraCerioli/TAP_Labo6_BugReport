using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BugReportData {
    public class BugReportContext:DbContext {
        protected override void OnConfiguring(DbContextOptionsBuilder options) {
            options.UseSqlServer(@"Data Source=.;Initial Catalog=TAP_EF_LAB;Integrated Security =True;");
            options.LogTo(Console.WriteLine).EnableSensitiveDataLogging();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            var u = modelBuilder.Entity<User>();
            u.OwnsOne(user => user.Address);
            var comm = modelBuilder.Entity<Comment>();
            comm.HasOne(c => c.Author).WithMany(u => u.Comments).OnDelete(DeleteBehavior.ClientCascade);
            var prod = modelBuilder.Entity<Product>();
            prod.HasMany(p => p.IncompatibleWith).WithMany("IncompatibleOtherSide").UsingEntity(pp=>pp.ToTable("Incompatibilities"));
            prod.HasMany(p => p.DependsOn).WithMany(p => p.NeededBy).UsingEntity(pp=>pp.ToTable("Dependencies"));
        }

        public DbSet<User> Users { get; set; }
    }

    public class Address {
        public string Town { get; set; }
        [MinLength(3)]
        [MaxLength(100)]
        public string Street { get; set; }
        public int DoorNumber { get; set; }
    }
    public enum Status{Open,InProgress,Closed}
    [Index(nameof(Login),IsUnique = true,Name = "LoginUnique")]
    [Index(nameof(FiscalCode),IsUnique = true,Name="FiscalCodeUnique")]
    public class User {
        public int UserId { get; set; }
        [MinLength(8)]
        [MaxLength(32)]
        public string Login { get; set; }
        [MinLength(3)]
        [MaxLength(50)]
        public string Name { get; set; }
        [MinLength(2)]
        [MaxLength(256)]
        public string FamilyName { get; set; }
        public DateTime Birthdate { get; set; }
        [RegularExpression(@"\w{6}\d{2}\w\d{2}\w\d{3}\w")]
        [MaxLength(16)]
        [MinLength(16)]
        public string FiscalCode { get; set; }
        [NotMapped]
        public int Age => DateTime.Now.Year - Birthdate.Year;
        public Address? Address { get; set; }
        public List<Report> Reports { get; set; }
        public List<Comment> Comments { get; set; }
    }
    [Index(nameof(Code),IsUnique = true,Name="CodeUnique")]
    public class Product {
        public int ProductId { get; set; }
        public int Code { get; set; }
        public string CommercialName { get; set; }
        public string InternalReference { get; set; }
        public string? Description { get; set; }
        public List<Product> IncompatibleWith { get; set; }
        public List<Product> DependsOn { get; set; }
        public List<Product> NeededBy { get; set; }
        public List<Report> Reports { get; set; }
    }

    public class Report {
        public int ReportId { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(256)]
        public string ShortDescription { get; set; }
        public string? Text { get; set; }
        public User? Author { get; set; }
        public int UserId { get; set; }
        public Product? Product { get; set; }
        public int ProductId { get; set; }
        public List<Comment> Comments { get; set; }
    }

    public class Comment {
        public int CommentId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Text { get; set; }
        public User? Author { get; set; }
        public int UserId { get; set; }
        public Report? Report { get; set; }
        public int ReportId { get; set; }
    }
}
