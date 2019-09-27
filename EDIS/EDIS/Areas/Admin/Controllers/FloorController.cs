using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Data;
using EDIS.Models.LocationModels;
using EDIS.Models.Identity;
using EDIS.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace EDIS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class FloorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public FloorController(ApplicationDbContext context,
                               IRepository<AppUserModel, int> userRepo,
                               CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        // GET: Admin/Floor
        public async Task<IActionResult> Index(int? buildingId)
        {
            ViewBag.BuildingId = new SelectList(_context.Buildings, "BuildingId", "BuildingName", buildingId);
            return View(await _context.Floors.ToListAsync());
        }

        // GET: Admin/Floor/GetFloorList/5
        public IActionResult GetFloorList(int? buildingId)
        {
            ViewBag.BuildingName = _context.Buildings.Find(buildingId).BuildingName;
            ViewBag.BuildingId = buildingId;
            var floorList = _context.Floors.Where(f => f.BuildingId == buildingId);

            /* Get user account to show. */
            foreach (var item in floorList)
            {
                item.RtpName = _context.AppUsers.Find(item.Rtp).UserName;
            }
            return View(floorList.ToList());
        }

        // GET: Admin/Floor/Create/5
        public IActionResult Create(int buildingId)
        {
            ViewBag.BuildingName = _context.Buildings.Find(buildingId).BuildingName;
            // Set default value.
            FloorModel floorModel = new FloorModel()
            {
                BuildingId = buildingId,
                Flg = "Y"
            };
            return View(floorModel);
        }

        // POST: Admin/Floor/Create/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int buildingId, [Bind("BuildingId,FloorId,FloorName,Flg")] FloorModel floorModel)
        {
            ViewBag.BuildingName = _context.Buildings.Find(buildingId).BuildingName;
            // Get Login User's details.
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            // Set values.
            floorModel.Rtp = ur.Id;
            floorModel.Rtt = DateTime.Now;
            
            var lastFloor = _context.Floors.Where(f => f.BuildingId == buildingId);
            if(lastFloor.Count() == 0)
            {
                // If the building hasn't any floor.
                var lastFloorId = (buildingId * 100) + 1;
                floorModel.FloorId = lastFloorId.ToString();
            }
            else
            {
                var lastFloorId = lastFloor.OrderByDescending(f => f.FloorId).First().FloorId;
                floorModel.FloorId = (System.Convert.ToInt32(lastFloorId) + 1).ToString();
            }                   

            if (ModelState.IsValid)
            {
                _context.Add(floorModel);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { BuildingId = buildingId });
            }
            return View(floorModel);
        }

        // GET: Admin/Floor/Edit/5
        public async Task<IActionResult> Edit(int? buildingId, string floorId)
        {
            ViewBag.BuildingName = _context.Buildings.Find(buildingId).BuildingName;
            if (buildingId == null || floorId == null)
            {
                return NotFound();
            }

            var floorModel = await _context.Floors.FindAsync(buildingId, floorId);
            if (floorModel == null)
            {
                return NotFound();
            }
            return View(floorModel);
        }

        // POST: Admin/Floor/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int buildingId, string floorId, [Bind("BuildingId,FloorId,FloorName,Flg,Rtp,Rtt")] FloorModel floorModel)
        {
            ViewBag.BuildingName = _context.Buildings.Find(buildingId).BuildingName;
            // Get Login User's details.
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            floorModel.Rtp = ur.Id;
            floorModel.Rtt = DateTime.Now;

            if (buildingId != floorModel.BuildingId || floorId != floorModel.FloorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(floorModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FloorModelExists(floorModel.BuildingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", new { BuildingId = buildingId });
            }
            return View(floorModel);
        }

        private bool FloorModelExists(int id)
        {
            return _context.Floors.Any(e => e.BuildingId == id);
        }
    }
}
