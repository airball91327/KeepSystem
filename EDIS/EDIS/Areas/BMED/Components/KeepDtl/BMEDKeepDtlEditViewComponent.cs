using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Components.KeepDtl
{
    public class BMEDKeepDtlEditViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public BMEDKeepDtlEditViewComponent(BMEDDbContext context,
                                            IRepository<AppUserModel, int> userRepo,
                                            CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            var ur = _userRepo.Find(us => us.UserName == this.User.Identity.Name).FirstOrDefault();

            // InOut dropdown list's items.
            List<SelectListItem> listItem1 = new List<SelectListItem>();
            listItem1.Add(new SelectListItem { Text = "自行", Value = "0" });
            listItem1.Add(new SelectListItem { Text = "委外", Value = "1" });
            listItem1.Add(new SelectListItem { Text = "租賃", Value = "2" });
            listItem1.Add(new SelectListItem { Text = "保固", Value = "3" });
            ViewData["InOut"] = new SelectList(listItem1, "Value", "Text", "");

            // Result dropdown list's items.
            List<SelectListItem> listItem2 = new List<SelectListItem>();
            _context.BMEDKeepResults.Where(d => d.Flg == "Y")
                    .ToList()
                    .ForEach(d => {
                         listItem2.Add(new SelectListItem { Text = d.Title, Value = d.Id.ToString() });
                    });
            ViewData["Result"] = new SelectList(listItem2, "Value", "Text", "");

            if (id == null)
            {
                KeepDtlModel dtl = new KeepDtlModel();
                dtl.IsCharged = "N";
                return View(dtl);
            }

            KeepDtlModel keepDtl = _context.BMEDKeepDtls.Find(id);
            KeepFlowModel kf = _context.BMEDKeepFlows.Where(f => f.DocId == id)
                                                     .Where(f => f.Status == "?").FirstOrDefault();

            /* Get CheckerName from Repair table. */
            var checkerId = _context.BMEDKeeps.Find(id).CheckerId;
            keepDtl.CheckerName = checkerId == 0 ? "" : _context.AppUsers.Find(checkerId).FullName;

            var isEngineer = _context.UsersInRoles.Where(u => u.AppRoles.RoleName == "MedEngineer" &&
                                                              u.UserId == ur.Id).FirstOrDefault();
            if (kf.Cls.Contains("工程師") && kf.UserId == ur.Id)  /* 流程 => 工程師，Login User => 負責之工程師 */
            {
                return View(keepDtl);
            }
            else if (kf.Cls.Contains("工程師") && isEngineer != null)  /* 流程 => 工程師，Login User => 非負責之工程師 */
            {
                return View(keepDtl);
            }
            else  /* 流程 => 其他 */
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
                keepDtl.ResultTitle = keepDtl.Result == null ? "" : _context.BMEDKeepResults.Find(keepDtl.Result).Title;
                return View("Details", keepDtl);
            }
        }
    }
}
