using System;
using Microsoft.AspNetCore.Identity;

namespace EDIS.Models.Identity
{
	public class ApplicationRole : IdentityRole
    {
        public AppRoleModel approle;
        public ApplicationRole()
        {
        }
    }
}
