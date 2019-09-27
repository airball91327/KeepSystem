using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDIS.Models.Identity
{
    public class UserInRolesViewModel
    { 
        public string RoleName { get; set; }
        public string Description { get; set; }
        public bool IsSelected { get; set; }
    }
}
