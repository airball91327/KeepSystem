using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.RepairModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.RepairEmp
{
    public class RepEmpList2ViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public RepEmpList2ViewComponent(ApplicationDbContext context,
                                        IRepository<AppUserModel, int> userRepo,
                                        CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            AppUserModel u;
            List<RepairEmpModel> emps = _context.RepairEmps.Where(p => p.DocId == id).ToList();
            emps.ForEach(rp =>
            {
                rp.UserName = (u = _context.AppUsers.Find(rp.UserId)) == null ? "" : u.UserName;
                rp.FullName = (u = _context.AppUsers.Find(rp.UserId)) == null ? "" : u.FullName;
            });

            return View(emps);
        }
    }
}
