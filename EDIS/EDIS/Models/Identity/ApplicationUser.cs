using System;
using Microsoft.AspNetCore.Identity;

namespace EDIS.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public AppUserModel appuser;
        public ApplicationUser()
        {
        }
    }
}
