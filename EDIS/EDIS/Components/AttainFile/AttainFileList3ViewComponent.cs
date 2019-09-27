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

namespace EDIS.Components.AttainFile
{
    public class AttainFileList3ViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public AttainFileList3ViewComponent(ApplicationDbContext context,
                                            IRepository<AppUserModel, int> userRepo,
                                            CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id = null, string typ = null)
        {
            List<AttainFileModel> af = new List<AttainFileModel>();
            AppUserModel u;
            af = _context.AttainFiles.Where(f => f.DocType == typ).Where(f => f.DocId == id).ToList();
            foreach (AttainFileModel a in af)
            {
                u = _context.AppUsers.Find(a.Rtp);
                if (u != null)
                    a.UserName = u.FullName;
            }
            return View(af);
        }
    }
}
