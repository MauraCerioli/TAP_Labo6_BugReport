using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BugReportData {
    // Exceptions belong in a different project, to be referenced by all tiers
    // we put them here as a shortcut, but it is not a good design
    [Serializable]
    public class BRException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public BRException() { }
        public BRException(string message) : base(message) { }
        public BRException(string message, Exception inner) : base(message, inner) { }

        protected BRException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class UnauthorizedException : BRException {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public UnauthorizedException() { }
        public UnauthorizedException(string message) : base(message) { }
        public UnauthorizedException(string message, Exception inner) : base(message, inner) { }

        protected UnauthorizedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class UnavailableDbException : BRException {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public UnavailableDbException() { }
        public UnavailableDbException(string message) : base(message) { }
        public UnavailableDbException(string message, Exception inner) : base(message, inner) { }

        protected UnavailableDbException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class UniquenessViolationException : BRException {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public UniquenessViolationException() { }
        public UniquenessViolationException(string message) : base(message) { }
        public UniquenessViolationException(string message, Exception inner) : base(message, inner) { }

        protected UniquenessViolationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class UserUnknownException : BRException {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public UserUnknownException() { }
        public UserUnknownException(string message) : base(message) { }
        public UserUnknownException(string message, Exception inner) : base(message, inner) { }

        protected UserUnknownException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
    public class BugReportContext:DbContext {
        public BugReportContext(string connectionString):base(new DbContextOptionsBuilder<BugReportContext>().UseSqlServer(connectionString).Options){}
        protected override void OnConfiguring(DbContextOptionsBuilder options) {
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

        public override int SaveChanges() {
            try {
                return base.SaveChanges();
            }
            catch (SqlException e) {
                throw new UnavailableDbException("Unavailable Db", e);
            }
            catch (DbUpdateException e) {
                var sqlException = e.InnerException as SqlException;
                if (null==sqlException) throw new BRException("Missing information form Db exception", e);
                switch (sqlException.Number) {
                    case 2601: throw new UniquenessViolationException("TODO get info from sqlException for informative error message",e);
                    //all cases for other possible error codes
                    default:
                        throw new BRException("Missing information form Db exception", e);
                }
            }
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
        public string Password { get; set; }//hash della password
        public bool IsAdmin { get; set; }
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

        public User(string login, string password, bool isAdmin,string name, string familyName, DateTime birthdate, string fiscalCode) {
            Login = login;
            Password = password;
            Name = name;
            FamilyName = familyName;
            Birthdate = birthdate;
            FiscalCode = fiscalCode;
            Reports = new List<Report>();
            Comments = new List<Comment>();
        }
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
        public Product(int code, string commercialName, string internalReference, string? description) {
            Code = code;
            CommercialName = commercialName;
            InternalReference = internalReference;
            Description = description;
            IncompatibleWith = new List<Product>();
            DependsOn = new List<Product>();
            NeededBy = new List<Product>();
            Reports = new List<Report>();
        }
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
        public Report(Status status, DateTime createdOn, string shortDescription, string? text, int userId, int productId) {
            Status = status;
            CreatedOn = createdOn;
            ShortDescription = shortDescription;
            Text = text;
            UserId = userId;
            ProductId = productId;
            Comments = new List<Comment>();
        }
    }

    public class Comment {
        public int CommentId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Text { get; set; }
        public User? Author { get; set; }
        public int UserId { get; set; }
        public Report? Report { get; set; }
        public int ReportId { get; set; }
        public Comment(DateTime createdOn, string text, int userId, int reportId) {
            CreatedOn = createdOn;
            Text = text;
            UserId = userId;
            ReportId = reportId;
        }
    }
}
