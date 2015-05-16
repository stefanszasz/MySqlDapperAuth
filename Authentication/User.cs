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

        public string UserId
        {
            get { return Id; }
            set { Id = value; }
        }

        internal async Task<System.Security.Claims.ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }

        public bool EmailConfirmed { get; set; }
    }
}