using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BugReportData;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace BugReportLogic {
    public static class BugReportConstants {
        public const int MinPwLength = 8;
        public const string OwnerLogin = "BugReportOwner";

        public static string HashPassword(string pw) {
            //https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
            //PBKDF2 classe Rfc2898DeriveBytes
            //PER EVITARE DI FARE TUTTO IO FACCIO UN ORRORE
            return pw;
        }
    }
    public class Session {
        public string ConnectionString { get;  }
        public int UserId { get;  }
        public bool IsAdmin { get;  }
        public Session(string connectionString, int userId, bool isAdmin) {
            ConnectionString = connectionString;
            UserId = userId;
            IsAdmin = isAdmin;
        }

        public void AddUser(string login, string password, bool isAdmin, string name, string familyName,
            DateTime birthdate, string fiscalCode, Address? address) {
            if (!IsAdmin) throw new UnauthorizedException("only admin can add users");
            var user = new User(login, BugReportConstants.HashPassword(password), isAdmin, name, familyName, birthdate,
                fiscalCode) { Address = address };

        }
    }

    public class BugReportSystem {
        public string ConnectionString { get; }
        public BugReportSystem(string connectionString) {
            ConnectionString = connectionString;
        }

        public Session Login(string login, string pw) {
            //TODO verify parameters
            using (var c = new BugReportContext(ConnectionString)) {
                try {
                    var owner = c.Users.Single(u =>
                        u.Login == login && u.Password == BugReportConstants.HashPassword(pw));
                    return new Session(ConnectionString, owner.UserId,owner.IsAdmin);
                }
                catch (SqlException e) {
                    throw new UnavailableDbException("Unavailable Db", e);
                }
                catch (DbUpdateException e) {
                    //TODO verify that codes etc confirm my guess
                    throw new UserUnknownException("no user with these credentials", e);
                }
            }
        }
    }

    public static class BugReportFactory {
        static void ParameterVerify(string connectionString, string connStringName,string adminPassword, string pwName) {
            if (String.IsNullOrEmpty(connectionString))
                throw new ArgumentException("connection strings cannot be null or empty", connStringName);
            if (String.IsNullOrEmpty(adminPassword))
                throw new ArgumentException("passwords cannot be empty or null", pwName);
            if (BugReportConstants.MinPwLength>adminPassword.Length)
                throw new ArgumentException($"password must be at least {BugReportConstants.MinPwLength} long", pwName);
        }


        public static BugReportSystem InitializeBugTracking(string connectionString, string adminPassword) {
            ParameterVerify(connectionString, nameof(connectionString), adminPassword, nameof(adminPassword));
            using (var c= new BugReportContext(connectionString)) {
                c.Database.EnsureDeleted();
                c.Database.EnsureCreated();
                var owner = new User(BugReportConstants.OwnerLogin, BugReportConstants.HashPassword(adminPassword), true,"NAME", "FAMILYNAME",
                    DateTime.Now, "XYWXYW00Z99W000H");
                c.SaveChanges();
            }
            return new BugReportSystem(connectionString);
        }

        public static BugReportSystem LoadBugTracking(string connectionString, string adminPassword) {
            ParameterVerify(connectionString, nameof(connectionString), adminPassword, nameof(adminPassword));
            using (var c= new BugReportContext(connectionString)) {
                try {
                    var owner = c.Users.Single(u =>
                        u.Login == BugReportConstants.OwnerLogin && u.Password == BugReportConstants.HashPassword(adminPassword));
                }
                catch (SqlException e) {
                    throw new UnavailableDbException("Unavailable Db", e);
                }
                catch (DbUpdateException e) {
                    //TODO verify that codes etc confirm my guess
                    throw new UserUnknownException("no admin with that password",e);
                }
                return new BugReportSystem(connectionString);
            }
        }
    }
}
