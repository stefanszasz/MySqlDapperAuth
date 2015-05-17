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
    public class UserStore : IUserStore<User>, IUserLoginStore<User>, IUserPasswordStore<User>, IUserSecurityStampStore<User>, IUserEmailStore<User>, IUserRoleStore<User>
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
                    connection.Execute("insert into Users(Id, UserName, PasswordHash, SecurityStamp, Email, FirstName, LastName) values(@Id, @userName, @passwordHash, @securityStamp, @email, @firstName, @lastName)", user);
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

        public Task AddToRoleAsync(User user, string roleName)
        {
            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                {
                    var role = connection.Query<Role>("select * from roles where name = @roleName", new { roleName })
                        .SingleOrDefault();
                    if (role == null)
                        throw new InvalidOperationException("Role " + roleName + " should be there");

                    connection.Execute("insert into UserRoles(UserId, RoleId) values(@userId, @roleId)", new { userId = user.Id, roleId = role.Id });
                }
            });
        }

        public Task RemoveFromRoleAsync(User user, string roleName)
        {
            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                {
                    var role = connection.Query<Role>("select * from roles where name = @roleName", new { roleName })
                        .SingleOrDefault();
                    if (role == null)
                        throw new InvalidOperationException("Role " + roleName + " should be there");

                    connection.Execute("delete UserRoles where UserId=@userId and RoleId=@roleId", new { userId = user.Id, roleId = role.Id });
                }
            });
        }

        public Task<IList<string>> GetRolesAsync(User user)
        {
            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                {
                    var userRoles = connection.Query<dynamic>("select * from UserRoles where UserId = @userId", new { userId = user.Id })
                                         .ToArray();
                    if (!userRoles.Any()) 
                        return new List<string>();

                    var roleNames = userRoles.Select(x => x.RoleId).ToArray();
                    var roles = connection.Query<Role>("select * from Roles where Id IN (@roleIds)", new { roleIds = roleNames }).ToList();
                    IList<string> names = roles.Select(x => x.Name).ToList();
                    return names;
                }
            });
        }

        public Task<bool> IsInRoleAsync(User user, string roleName)
        {
            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                {
                    var role = connection.Query<Role>("select * from roles where name = @roleName", new { roleName })
                        .SingleOrDefault();
                    if (role == null)
                        throw new InvalidOperationException("Role " + roleName + " should be there");

                    var existingRoleAssignments = connection.Query<dynamic>("select * from UserRoles where RoleId = @roleId and UserId = @userId",
                        new {roleId = role.Id, userId = user.Id}).ToList();
                    bool isAssigned= existingRoleAssignments.Select(x => x.RoleId).Contains(role.Name);
                    return isAssigned;
                }
            });
        }
    }
}