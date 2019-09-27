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

namespace EDIS.Areas.BMED.Components.AttainFile
{
    public class BMEDAttainFileListViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public BMEDAttainFileListViewComponent(BMEDDbContext context,
                                               IRepository<AppUserModel, int> userRepo,
                                               CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string viewType, string id = null, string typ = null)
        {
            List<AttainFileModel> af = new List<AttainFileModel>();
            AppUserModel u;
            af = _context.BMEDAttainFiles.Where(f => f.DocType == typ).Where(f => f.DocId == id).ToList();
            foreach (AttainFileModel a in af)
            {
                u = _context.AppUsers.Find(a.Rtp);
                if (u != null)
                    a.UserName = u.FullName;
            }

            if (viewType == "Edit")
            {
                return View(af);
            }
            else if (viewType == "AjaxView")
            {
                return View("AjaxView", af);
            }
            return View("List2", af);
        }
    }
}
