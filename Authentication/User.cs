using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace MySqlDapperAuth.Authentication
{
    public class User : IUser
    {
        public string Id { get; private set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public User()
        {
            Id = Guid.NewGuid().ToString();
        }

        internal async Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager, string authenticationType)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            return userIdentity;
        }

        public bool EmailConfirmed { get; set; }
    }
}