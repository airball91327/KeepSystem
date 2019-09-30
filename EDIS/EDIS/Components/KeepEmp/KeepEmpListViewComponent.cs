using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.KeepModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.KeepEmp
{
    public class BMEDKeepEmpListViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public BMEDKeepEmpListViewComponent(ApplicationDbContext context,
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
            List<KeepEmpModel> emps = _context.KeepEmps.Where(p => p.DocId == id).ToList();
            emps.ForEach(rp =>
            {
                rp.UserName = (u = _context.AppUsers.Find(rp.UserId)) == null ? "" : u.UserName;
                rp.FullName = (u = _context.AppUsers.Find(rp.UserId)) == null ? "" : u.FullName;
            });

            if (viewType.Contains("Edit"))
            {
                return View(emps);
            }
            return View("List2", emps);
        }
    }
}
