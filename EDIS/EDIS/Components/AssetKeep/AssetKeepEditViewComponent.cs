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

namespace EDIS.Components.AssetKeep
{
    public class AssetKeepEditViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;

        public AssetKeepEditViewComponent(ApplicationDbContext context,
                                          IRepository<AppUserModel, int> userRepo,
                                          CustomUserManager customUserManager,
                                          CustomRoleManager customRoleManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
            roleManager = customRoleManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id, string viewType = null)
        {
            if (viewType == "Details")
            {
                if (id == null)
                {
                    return Content("無資料!");  
                }
                AssetKeepModel assetKeep = _context.AssetKeeps.Find(id);
                if (assetKeep == null)
                {
                    return Content("無資料!");
                }
                assetKeep.KeepEngName = assetKeep.KeepEngId == 0 ? "" : _context.AppUsers.Find(assetKeep.KeepEngId).FullName;
                return View("Details", assetKeep);
            }
            else
            {
                List<SelectListItem> listItem = new List<SelectListItem>();
                AppUserModel u;
                //db.AppUsers.ToList().ForEach(d =>
                //{
                //    listItem.Add(new SelectListItem { Text = d.FullName, Value = d.Id.ToString() });
                //});

                // Get MedEngineers to set dropdownlist.
                var s = roleManager.GetUsersInRole("RepEngineer").ToList();
                foreach (string l in s)
                {
                    u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                    if (u != null)
                    {
                        listItem.Add(new SelectListItem { Text = u.FullName, Value = u.Id.ToString() });
                    }
                }
                ViewData["KeepEngId"] = new SelectList(listItem, "Value", "Text");

                List<SelectListItem> listItem2 = new List<SelectListItem>();
                listItem2.Add(new SelectListItem { Text = "自行", Value = "自行" });
                listItem2.Add(new SelectListItem { Text = "委外", Value = "委外" });
                listItem2.Add(new SelectListItem { Text = "保固", Value = "保固" });
                listItem2.Add(new SelectListItem { Text = "租賃", Value = "租賃" });
                ViewData["InOut"] = new SelectList(listItem2, "Value", "Text", "");
                //
                List<SelectListItem> listItem3 = new List<SelectListItem>();
                _context.KeepFormats.ToList()
                    .ForEach(x =>
                    {
                        listItem3.Add(new SelectListItem { Text = x.FormatId + "(" + x.Plants + ")", Value = x.FormatId });
                    });
                ViewData["FormatId"] = new SelectList(listItem3, "Value", "Text", "");
                //
                if (id == null)
                {
                    return View();
                }
                AssetKeepModel assetKeep = _context.AssetKeeps.Find(id);
                if (assetKeep == null)
                {
                    assetKeep = new AssetKeepModel();
                    assetKeep.DeviceNo = id;
                }
                return View(assetKeep);
            }
        }
    }
}
