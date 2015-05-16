using System;
using Microsoft.AspNet.Identity;

namespace MySqlDapperAuth.Authentication
{
    public class Role : IRole
    {
        public string Id { get; private set; }
        public string Name { get; set; }

        public Role()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}