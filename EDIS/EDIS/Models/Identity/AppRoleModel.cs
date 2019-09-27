using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EDIS.Models.Identity
{
    public class AppRoleModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }
        [Required]
        [Display(Name = "角色名稱")]
        public string RoleName { get; set; }
        [Display(Name = "描述")]
        public string Description { get; set; }
    }
}
