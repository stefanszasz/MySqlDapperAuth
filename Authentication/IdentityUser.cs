using System.Collections.Generic;
using MySqlDapperAuth.Models;

namespace MySqlDapperAuth.Authentication
{
    public class IdentityUser : User
    {
        public IdentityUser()
        {
            Logins = new List<IdentityUserLogin>();
        }

        public string Email { get; set; }

        public IList<IdentityUserLogin> Logins { get; set; }
    }
}