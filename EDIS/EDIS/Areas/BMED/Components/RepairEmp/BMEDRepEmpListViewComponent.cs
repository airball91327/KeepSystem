using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Components.RepairEmp
{
    public class BMEDRepEmpListViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public BMEDRepEmpListViewComponent(BMEDDbContext context,
                                           IRepository<AppUserModel, int> userRepo,
                                           CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id, string viewType)
        {
            AppUserModel u;
            List<RepairEmpModel> emps = _context.BMEDRepairEmps.Where(p => p.DocId == id).ToList();
            emps.ForEach(rp =>
            {
                rp.UserName = (u = _context.AppUsers.Find(rp.UserId)) == null ? "" : u.UserName;
                rp.FullName = (u = _context.AppUsers.Find(rp.UserId)) == null ? "" : u.FullName;
            });

            if(viewType.Contains("Edit"))
            {
                return View(emps);
            }
            return View("List2", emps);
        }
    }
}
