using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDIS.Models.Identity
{
    public class UsersInRolesModel
    {
        [Key, Column(Order = 1)]
        public int UserId { get; set; }
        public string UserName { get; set; }
        [Key, Column(Order = 2)]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual AppRoleModel AppRoles { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUserModel AppUsers { get; set; }
    }
}
