using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNet.Identity;
using MySql.Data.MySqlClient;

namespace MySqlDapperAuth.Authentication
{
    public class UserStore : IUserStore<User>, IUserLoginStore<User>, IUserPasswordStore<User>, IUserSecurityStampStore<User>, IUserEmailStore<User>
    {
        private readonly string connectionString;

        public UserStore(string connectionStringName)
        {
            if (string.IsNullOrWhiteSpace(connectionStringName))
                throw new ArgumentNullException("connectionStringName");

            connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
        }

        public void Dispose()
        {

        }

        public virtual Task CreateAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                {
                    connection.Open();
                    using (var tx = connection.BeginTransaction())
                    {
                        var userRole = connection.Query<Role>("select * from Roles where Name = 'User'", transaction:tx).SingleOrDefault();
                        if (userRole == null)
                            throw new InvalidDataException("User role not available in the database. It should be.");

                        connection.Execute("insert into Users(Id, UserName, PasswordHash, SecurityStamp, Email, FirstName, LastName) values(@Id, @userName, @passwordHash, @securityStamp, @email, @firstName, @lastName)", user, tx);
                        connection.Execute("insert into UserRoles(UserId, RoleId) values(@userId, @roleId)", new { userId = user.Id, roleId = userRole.Id }, tx);

                        tx.Commit();
                    }
                }
            });
        }

        public virtual Task DeleteAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                    connection.Execute("delete from Users where Id = @Id", new { user.Id });
            });
        }

        public virtual Task<User> FindByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("id");

            Guid parsedId;
            if (!Guid.TryParse(id, out parsedId))
                throw new ArgumentOutOfRangeException("id", string.Format("'{0}' is not a valid GUID.", new { id }));

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                {
                    return connection.Query<User>("select * from Users where Id = @Id", new { Id = parsedId }).SingleOrDefault();
                }
            });
        }

        public virtual Task<User> FindByNameAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException("userName");

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                {
                    var user = connection.Query<User>("select * from Users where LOWER(UserName) = LOWER(@userName)", new { userName }).SingleOrDefault();
                    return user;
                }
            });
        }

        public virtual Task UpdateAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                    connection.Execute("update Users set UserName = @userName, PasswordHash = @passwordHash, SecurityStamp = @securityStamp, FirstName = @firstName, LastName = @lastName where Id = @Id", user);
            });
        }

        public virtual Task AddLoginAsync(User user, UserLoginInfo login)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (login == null)
                throw new ArgumentNullException("login");

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                    connection.Execute("insert into ExternalLogins(Id, UserId, LoginProvider, ProviderKey) values(@id, @UserId, @loginProvider, @providerKey)",
                        new { externalLoginId = Guid.NewGuid(), UserId = user.Id, loginProvider = login.LoginProvider, providerKey = login.ProviderKey });
            });
        }

        public virtual Task<User> FindAsync(UserLoginInfo login)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                    return connection.Query<User>("select u.* from Users as u inner join ExternalLogins as l on l.Id = u.Id where l.LoginProvider = @loginProvider and l.ProviderKey = @providerKey",
                        login).SingleOrDefault();
            });
        }

        public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                    return (IList<UserLoginInfo>)connection.Query<UserLoginInfo>("select LoginProvider, ProviderKey from ExternalLogins where Id = @Id", new { user.Id }).ToList();
            });
        }

        public virtual Task RemoveLoginAsync(User user, UserLoginInfo login)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (login == null)
                throw new ArgumentNullException("login");

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                    connection.Execute("delete from ExternalLogins where Id = @id and LoginProvider = @loginProvider and ProviderKey = @providerKey",
                        new { user.Id, login.LoginProvider, login.ProviderKey });
            });
        }

        public virtual Task<string> GetPasswordHashAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PasswordHash);
        }

        public virtual Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        public virtual Task SetPasswordHashAsync(User user, string passwordHash)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.PasswordHash = passwordHash;

            return Task.FromResult(0);
        }

        public virtual Task<string> GetSecurityStampAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.SecurityStamp);
        }

        public virtual Task SetSecurityStampAsync(User user, string stamp)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.SecurityStamp = stamp;

            return Task.FromResult(0);
        }

        public Task SetEmailAsync(User user, string email)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.Email = email;

            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<User> FindByEmailAsync(string email)
        {
            if (email == null)
                throw new ArgumentNullException("email");

            return Task.Factory.StartNew(() =>
            {
                using (var connection = BuildNewConnection())
                {
                    return connection.Query<User>("select * from Users where Email = @email", new { email }).SingleOrDefault();
                }
            });
        }

        private MySqlConnection BuildNewConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}