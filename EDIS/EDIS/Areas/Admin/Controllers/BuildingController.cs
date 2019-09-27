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
    public class BuildingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public BuildingController(ApplicationDbContext context,
                                  IRepository<AppUserModel, int> userRepo,
                                  CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        // GET: Admin/Building
        public async Task<IActionResult> Index()
        {
            var buildingList = _context.Buildings;
            /* Get user account to show. */
            foreach (var item in buildingList)
            {
                item.RtpName = _context.AppUsers.Find(item.Rtp).UserName;
            }
            return View(await buildingList.ToListAsync());
        }

        // GET: Admin/Building/Create
        public IActionResult Create()
        {
            BuildingModel buildingModel = new BuildingModel()
            {
                Flg = "Y"
            };
            return View(buildingModel);
        }

        // POST: Admin/Building/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BuildingId,BuildingName,Flg")] BuildingModel buildingModel)
        {
            // Get Login User's details.
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            buildingModel.Rtp = ur.Id;
            buildingModel.Rtt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(buildingModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(buildingModel);
        }

        // GET: Admin/Building/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buildingModel = await _context.Buildings.SingleOrDefaultAsync(m => m.BuildingId == id);
            if (buildingModel == null)
            {
                return NotFound();
            }
            return View(buildingModel);
        }

        // POST: Admin/Building/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BuildingId,BuildingName,Flg")] BuildingModel buildingModel)
        {
            // Get Login User's details.
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            buildingModel.Rtp = ur.Id;
            buildingModel.Rtt = DateTime.Now;

            if (id != buildingModel.BuildingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(buildingModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BuildingModelExists(buildingModel.BuildingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(buildingModel);
        }

        private bool BuildingModelExists(int id)
        {
            return _context.Buildings.Any(e => e.BuildingId == id);
        }
    }
}
