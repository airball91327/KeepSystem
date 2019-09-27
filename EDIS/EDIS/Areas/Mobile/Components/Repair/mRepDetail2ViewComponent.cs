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

namespace EDIS.Areas.Mobile.Components.Repair
{
    public class mRepDetail2ViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public mRepDetail2ViewComponent(ApplicationDbContext context,
                                        IRepository<AppUserModel, int> userRepo,
                                        CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            RepairModel repair = _context.Repairs.Find(id);

            /* Get and set value for NotMapped fields. */
            repair.DptName = _context.Departments.Find(repair.DptId).Name_C;
            repair.AccDptName = _context.Departments.Find(repair.AccDpt).Name_C;
            repair.CheckerName = _context.AppUsers.Find(repair.CheckerId).FullName;
            if(repair.AssetNo != null && repair.AssetNo != "")
            {
                var tempAsset = _context.Assets.Where(a => a.AssetNo == repair.AssetNo).FirstOrDefault();
                if(tempAsset != null)
                {
                    repair.AssetAccDate = tempAsset.AccDate;
                }
            }
            if (!string.IsNullOrEmpty(repair.Building))
            {
                int buildingId = Convert.ToInt32(repair.Building);
                repair.BuildingName = _context.Buildings.Find(buildingId).BuildingName;
                if (!string.IsNullOrEmpty(repair.Floor))
                {
                    repair.FloorName = _context.Floors.Find(buildingId, repair.Floor).FloorName;
                    if (!string.IsNullOrEmpty(repair.Area))
                    {
                        repair.AreaName = _context.Places.Find(buildingId, repair.Floor, repair.Area).PlaceName;
                    }
                }
            }
            return View(repair);
        }

    }
}
