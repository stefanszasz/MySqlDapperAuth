using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNet.Identity;
using MySql.Data.MySqlClient;

namespace MySqlDapperAuth.Authentication
{
    public class RoleStore : IQueryableRoleStore<Role>
    {
        private readonly string connectionString;

        public RoleStore(string connectionStringName)
        {
            if (string.IsNullOrWhiteSpace(connectionStringName))
                throw new ArgumentNullException("connectionStringName");

            connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
        }

        public void Dispose()
        {
            
        }

        public Task CreateAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException("role");

            using (MySqlConnection connection = BuildNewConnection())
            {
                return connection.ExecuteAsync("insert into Roles(RoleId, Name) values(@roleId, @name)", role);
            }
        }

        public Task UpdateAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException("role");

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                    connection.Execute("update Roles set Name = @name, where RoleId = @roleId", role);
            });
        }

        public Task DeleteAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException("role");

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                    connection.Execute("delete from Roles where RoleId = @roleId", new { role.Id });
            });
        }

        public Task<Role> FindByIdAsync(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentNullException("roleId");

            Guid parsedRoleId;
            if (!Guid.TryParse(roleId, out parsedRoleId))
                throw new ArgumentOutOfRangeException("roleId", string.Format("'{0}' is not a valid GUID.", new { roleId }));

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                    return connection.Query<Role>("select * from Role where RoleId = @roleId", new { roleId = parsedRoleId }).SingleOrDefault();
            });
        }

        public Task<Role> FindByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException("roleName");

            return Task.Factory.StartNew(() =>
            {
                using (MySqlConnection connection = BuildNewConnection())
                    return connection.Query<Role>("select * from Role where Name = @roleName", new { roleName }).SingleOrDefault();
            });
        }

        private MySqlConnection BuildNewConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public IQueryable<Role> Roles
        {
            get
            {
                using (MySqlConnection connection = BuildNewConnection())
                {
                    return connection.Query<Role>("select * from Roles")
                                     .ToList()
                                     .AsQueryable();
                }
            }
        }
    }
}