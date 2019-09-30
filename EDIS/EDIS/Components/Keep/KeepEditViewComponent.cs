using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.KeepModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.Keep
{
    public class KeepEditViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public KeepEditViewComponent(ApplicationDbContext context,
                                     IRepository<AppUserModel, int> userRepo,
                                     CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            KeepModel kp = _context.Keeps.Find(id);
            kp.AccDptName = kp.AccDpt == null ? "" : _context.Departments.Find(kp.AccDpt).Name_C;
            kp.CheckerName = kp.CheckerId == 0 ? "" : _context.AppUsers.Find(kp.CheckerId).FullName;

            return View(kp);
        }
    }
}
