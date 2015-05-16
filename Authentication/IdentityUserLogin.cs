namespace MySqlDapperAuth.Authentication
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.

    public class IdentityUserLogin
    {
        public string LoginProvider { get; set; }

        public string ProviderKey { get; set; }
    }
}