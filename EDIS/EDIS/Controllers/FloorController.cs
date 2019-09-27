using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Models.LocationModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Controllers
{
    [Authorize]
    public class FloorController : Controller
    {
        private readonly IRepository<BuildingModel, int> _buildRepo;
        private readonly IRepository<FloorModel, string[]> _floorRepo;

        public FloorController(IRepository<BuildingModel, int> buildRepo, 
                               IRepository<FloorModel, string[]> floorRepo)
        {
            _buildRepo = buildRepo;
            _floorRepo = floorRepo;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetFloors(int bname)
        {
            //int buildingId = _buildRepo.Find(b => b.BuildingName == bname).FirstOrDefault().BuildingId;
            List<SelectListItem> list = new List<SelectListItem>();
            _floorRepo.Find(f => f.BuildingId == bname && f.Flg == "Y").ToList()
                .ForEach(f => {
                    list.Add(new SelectListItem { Text=f.FloorName, Value=f.FloorId });
                });

            return Json(list);
        }
    }
}