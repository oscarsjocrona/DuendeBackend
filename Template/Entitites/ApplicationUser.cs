using Microsoft.AspNetCore.Identity;

namespace ids.Entitites
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() : base() { }
        public ApplicationUser(string id) : base(id) { }
        public string CustomerNumber { get; set; }

    }
}
