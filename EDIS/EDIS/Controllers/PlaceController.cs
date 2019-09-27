using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Data;
using EDIS.Models.LocationModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Controllers
{
    [Authorize]
    public class PlaceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlaceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetPlaces(int buildingId, string floorId)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            _context.Places.Where(f => f.BuildingId == buildingId &&
                                       f.FloorId == floorId && f.Flg == "Y").ToList()
                           .ForEach(f => {
                               list.Add(new SelectListItem { Text = f.PlaceName, Value = f.PlaceId });
                           });

            return Json(list);
        }
    }
}