using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Data;
using EDIS.Models.LocationModels;
using EDIS.Repositories;
using EDIS.Models.Identity;
using Microsoft.AspNetCore.Authorization;

namespace EDIS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class PlaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public PlaceController(ApplicationDbContext context,
                               IRepository<AppUserModel, int> userRepo,
                               CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        // GET: Admin/Place
        public async Task<IActionResult> Index(int? buildingId, string floorId)
        {
            ViewBag.BuildingId = new SelectList(_context.Buildings, "BuildingId", "BuildingName", buildingId);
            ViewBag.FloorId = floorId;
            return View(await _context.Places.ToListAsync());
        }

        // GET: Admin/Place/GetFloors/5
        public JsonResult GetFloors(int buildingId)
        {
            var floorsOfBuilding = _context.Floors.Where(f => f.BuildingId == buildingId);
            var floors = floorsOfBuilding.Select(m => new
            {
                floorId = m.FloorId,
                floorName = m.FloorName
            });
            return Json(floors);
        }

        // GET: Admin/Place/GetPlaceList/5
        public IActionResult GetPlaceList(int buildingId, string floorId)
        {
            ViewBag.BuildingName = _context.Buildings.Find(buildingId).BuildingName;
            ViewBag.BuildingId = buildingId;
            ViewBag.FloorName = _context.Floors.Find(buildingId, floorId).FloorName;
            ViewBag.FloorId = floorId;

            var placeList = _context.Places.Where(f => f.BuildingId == buildingId &&
                                                       f.FloorId == floorId);
            /* Get user account to show. */
            foreach(var item in placeList)
            {
                item.RtpName = _context.AppUsers.Find(item.Rtp).UserName;
            }
            return View(placeList.ToList());
        }

        // GET: Admin/Place/Details/5
        public async Task<IActionResult> Details(int? buildingId, string floorId, string placeId)
        {
            ViewBag.BuildingName = _context.Buildings.Find(buildingId).BuildingName;
            ViewBag.FloorName = _context.Floors.Find(buildingId, floorId).FloorName;

            if (buildingId == null)
            {
                return NotFound();
            }

            var placeModel = await _context.Places
                .SingleOrDefaultAsync(m => m.BuildingId == buildingId &&
                                           m.FloorId == floorId &&
                                           m.PlaceId == placeId);
            placeModel.RtpName = _context.AppUsers.Find(placeModel.Rtp).UserName;

            if (placeModel == null)
            {
                return NotFound();
            }

            return View(placeModel);
        }

        // GET: Admin/Place/Create/5
        public IActionResult Create(int buildingId, string floorId)
        {
            ViewBag.BuildingName = _context.Buildings.Find(buildingId).BuildingName;
            ViewBag.FloorName = _context.Floors.Find(buildingId, floorId).FloorName;
            // Set default value.
            PlaceModel placeModel = new PlaceModel()
            {
                BuildingId = buildingId,
                FloorId = floorId,
                Flg = "Y"
            };
            return View(placeModel);
        }

        // POST: Admin/Place/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BuildingId,FloorId,PlaceId,PlaceName,Flg,Rtp,Rtt")] PlaceModel placeModel)
        {
            ViewBag.BuildingName = _context.Buildings.Find(placeModel.BuildingId).BuildingName;
            ViewBag.FloorName = _context.Floors.Find(placeModel.BuildingId, placeModel.FloorId).FloorName;
            PlaceModel placeIdExist = _context.Places.Find(placeModel.BuildingId, placeModel.FloorId, placeModel.PlaceId);
            if (placeIdExist != null)
            {
                ModelState.AddModelError("PlaceId", "代號重複");
                return View(placeModel);
            }
            // Get Login User's details.
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            // Set values.
            placeModel.Rtp = ur.Id;
            placeModel.Rtt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(placeModel);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { buildingId = placeModel.BuildingId, floorId = placeModel.FloorId });
            }
            return View(placeModel);
        }

        // GET: Admin/Place/Edit/5
        public async Task<IActionResult> Edit(int? buildingId, string floorId, string placeId)
        {
            ViewBag.BuildingName = _context.Buildings.Find(buildingId).BuildingName;
            ViewBag.FloorName = _context.Floors.Find(buildingId, floorId).FloorName;

            if (buildingId == null)
            {
                return NotFound();
            }

            var placeModel = await _context.Places.SingleOrDefaultAsync(m => m.BuildingId == buildingId &&
                                                                             m.FloorId == floorId &&
                                                                             m.PlaceId == placeId);
            if (placeModel == null)
            {
                return NotFound();
            }
            return View(placeModel);
        }

        // POST: Admin/Place/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("BuildingId,FloorId,PlaceId,PlaceName,Flg,Rtp,Rtt")] PlaceModel placeModel)
        {
            ViewBag.BuildingName = _context.Buildings.Find(placeModel.BuildingId).BuildingName;
            ViewBag.FloorName = _context.Floors.Find(placeModel.BuildingId, placeModel.FloorId).FloorName;
            // Get Login User's details.
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            // Set values.
            placeModel.Rtp = ur.Id;
            placeModel.Rtt = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(placeModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlaceModelExists(placeModel.BuildingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", new { buildingId = placeModel.BuildingId, floorId = placeModel.FloorId });
            }
            return View(placeModel);
        }

        //GET: Admin/Place/CheckPlaceId
        /* Use ajax to check placeId. */
        public ActionResult CheckPlaceId(int buildingId, string floorId, string placeId)
        {
            var placeIdExist = _context.Places.Find(buildingId, floorId, placeId);

            string msg = "";
            if (placeIdExist != null)
            {
                msg = "<span style='color:red'>已有相同的代號</span>";
            }
            else
            {
                msg = "<span style='color:#4cff00'>可用的代號</span>";
            }
            return Json(msg);
        }

        private bool PlaceModelExists(int id)
        {
            return _context.Places.Any(e => e.BuildingId == id);
        }
    }
}
