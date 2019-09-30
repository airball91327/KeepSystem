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

namespace EDIS.Components.KeepDtl
{
    public class KeepDtlDetailViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public KeepDtlDetailViewComponent(ApplicationDbContext context,
                                          IRepository<AppUserModel, int> userRepo,
                                          CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            KeepDtlModel keepDtl = _context.KeepDtls.Find(id);
            if (keepDtl == null)
            {
                return Content("Page not found!");  //ViewComponent can't return HTTP response (like as StatusCode() or BasRequest())
            }
            else
            {
                switch (keepDtl.InOut)
                {
                    case "0":
                        keepDtl.InOut = "自行";
                        break;
                    case "1":
                        keepDtl.InOut = "委外";
                        break;
                    case "2":
                        keepDtl.InOut = "租賃";
                        break;
                    case "3":
                        keepDtl.InOut = "保固";
                        break;
                }
                if (keepDtl.Result != null)
                    keepDtl.ResultTitle = _context.KeepResults.Find(keepDtl.Result).Title;
            }
            /* Get CheckerName from Keep table. */
            var checkerId = _context.Keeps.Find(id).CheckerId;
            keepDtl.CheckerName = checkerId == 0 ? "" : _context.AppUsers.Find(checkerId).FullName;

            return View(keepDtl);
        }

    }
}
