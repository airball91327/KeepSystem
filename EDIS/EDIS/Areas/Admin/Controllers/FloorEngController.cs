using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Data;
using EDIS.Models.RepairModels;
using Microsoft.AspNetCore.Http;
using EDIS.Repositories;
using EDIS.Models.Identity;
using Microsoft.AspNetCore.Authorization;

namespace EDIS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class FloorEngController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public FloorEngController(ApplicationDbContext context, 
                                  IRepository<AppUserModel, int> userRepo,
                                  CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        // GET: Admin/FloorEng
        public async Task<IActionResult> Index()
        {
            /* Search all employees by department ID. */
            var DptEmps = _context.AppUsers.Where(a => a.DptId == "8410" || a.DptId == "8411" ||
                                                       a.DptId == "8412" || a.DptId == "8413" ||
                                                       a.DptId == "8414" || a.DptId == "8430");
            /* Insert search result into dropdownlist. */
            List<SelectListItem> engineerList = new List<SelectListItem>();
            foreach (var item in DptEmps)
            {
                engineerList.Add(new SelectListItem()
                {
                    Text = item.FullName,
                    Value = item.Id.ToString()
                });
            }

            var buildings = _context.Buildings;
            /* Insert search result into dropdownlist. */
            List<SelectListItem> buildingList = new List<SelectListItem>();
            foreach (var item in buildings)
            {
                buildingList.Add(new SelectListItem()
                {
                    Text = item.BuildingName,
                    Value = item.BuildingId.ToString()
                });
            }

            ViewData["EngineerId"] = engineerList;
            ViewData["BuildingId"] = buildingList;
            var applicationDbContext = _context.FloorEngs.Include(f => f.AppUsers).Include(f => f.Buildings).Include(f => f.Floors);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/FloorEng/GetEditList
        public async Task<IActionResult> GetEditList(string EngineerId, string BuildingId)
        {
            /* Left Outer Join table "Floors" and "FloorEngs". */
            var query = from f in _context.Floors
                        join eng in _context.FloorEngs
                        on
                            new { f.BuildingId, f.FloorId } // Join with multiple fields.
                        equals
                            new { eng.BuildingId, eng.FloorId } into subGrp
                        from s in subGrp.DefaultIfEmpty()
                        select new
                        {
                            EngId = (s == null ? 0 : s.EngId),                  // Outer Join need to deal "null" type
                            EngName = (s == null ? "N/A" : s.EngName),
                            FullName = (s == null ? "N/A" : s.AppUsers.FullName),
                            Rtp = (s == null ? 0 : s.Rtp),
                            Rtt = (s == null ? null : s.Rtt),
                            f.BuildingId,
                            f.FloorId,
                            f.FloorName
                        };
            var engList = query.ToList();

            /* Query "BuildingId". */
            if (BuildingId != null)
            {
                engList = engList.Where(e => e.BuildingId == Convert.ToInt32(BuildingId)).ToList();
            }
            /* Query "EngineerId". */
            if (EngineerId != null)
            {
                engList = engList.Where(e => e.EngId == Convert.ToInt32(EngineerId)).ToList();
            }

            /* Insert query result into floorEngList to display. */
            List<FloorEngListViewModel> floorEngList = new List<FloorEngListViewModel>();
            foreach (var item in engList)
            {
                int? engId = item.EngId;
                int? rtp = item.Rtp;
                string userName = "N/A";
                if (item.EngId == 0)    //If EngId is not found.
                {
                    engId = null;
                }
                if(item.Rtp == 0)       //If Rtp is not found.
                {
                    rtp = null;
                }
                if(_context.AppUsers.Find(item.Rtp) != null)    //If Rtp is not null.
                {
                    userName = _context.AppUsers.Find(item.Rtp).UserName;
                }
                floorEngList.Add(new FloorEngListViewModel()
                {
                    IsSelected = false,
                    BuildingId = item.BuildingId,
                    BuildingName = _context.Buildings.Find(item.BuildingId).BuildingName,
                    FloorId = item.FloorId,
                    FloorName = item.FloorName,
                    EngId = engId,
                    EngName = item.EngName,
                    EngFullName = item.FullName,
                    Rtp = rtp,
                    RtpName = userName,
                    Rtt = item.Rtt
                });
            }
            /* Order list by BuildingId then FloorId.*/
            floorEngList = floorEngList.OrderBy(f => f.BuildingId).ThenBy(f => Convert.ToInt32(f.FloorId)).ToList();

            /* Search all employees by department ID. */
            var DptEmps = _context.AppUsers.Where(a => a.DptId == "8410" || a.DptId == "8411" ||
                                                       a.DptId == "8412" || a.DptId == "8413" ||
                                                       a.DptId == "8414" || a.DptId == "8430");
            /* Insert search result into dropdownlist. */
            List<SelectListItem> engineerList = new List<SelectListItem>();
            foreach (var item in DptEmps)
            {
                engineerList.Add(new SelectListItem()
                {
                    Text = item.FullName,
                    Value = item.Id.ToString()
                });
            }
            ViewData["AsignEngId"] = engineerList;
            ViewData["QueryEngId"] = EngineerId;
            ViewData["QueryBuildingId"] = BuildingId;

            return View("EditList", floorEngList);
        }

        // POST: Admin/FloorEng/EditEngList
        [HttpPost]
        public async Task<IActionResult> EditEngList(List<FloorEngListViewModel> data, string AsignEngId,
                                                     string QueryEngId, string QueryBuildingId)
        {
            // Get Login User's details.
            var user = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            /* Target engineer ID. */
            int asignEngId = Convert.ToInt32(AsignEngId);

            /* Deal all input data. */
            foreach(var item in data)
            {
                /* Create or edit the selected row data. */
                if(item.IsSelected == true)
                {
                    /* Insert values to floorEngModel to save or create. */
                    FloorEngModel floorEngModel = new FloorEngModel
                    {
                        EngId = asignEngId,
                        EngName = _context.AppUsers.Find(asignEngId).UserName,
                        BuildingId = item.BuildingId,
                        FloorId = item.FloorId,
                        Rtp = user.Id,
                        Rtt = DateTime.UtcNow.AddHours(08)
                    };
                    /* If data isn't in the database, create data.*/
                    if (item.EngId == null)  
                    {
                        _context.Add(floorEngModel);
                    }
                    else
                    {
                        /* If data exist, find and delete odd data, then create a new one. */
                        int engId = item.EngId.GetValueOrDefault();
                        var originData = _context.FloorEngs.Find(engId, item.BuildingId, item.FloorId);                        
                        _context.FloorEngs.Remove(originData);
                        _context.FloorEngs.Add(floorEngModel);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("GetEditList", new { EngineerId = QueryEngId, BuildingId = QueryBuildingId });
        }

        // GET: Admin/FloorEng/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var floorEngModel = await _context.FloorEngs
                .Include(f => f.AppUsers)
                .Include(f => f.Buildings)
                .Include(f => f.Floors)
                .SingleOrDefaultAsync(m => m.EngId == id);
            if (floorEngModel == null)
            {
                return NotFound();
            }

            return View(floorEngModel);
        }

        // GET: Admin/FloorEng/Create
        public IActionResult Create()
        {
            ViewData["EngId"] = new SelectList(_context.AppUsers, "Id", "Email");
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "BuildingId", "BuildingId");
            ViewData["BuildingId"] = new SelectList(_context.Floors, "BuildingId", "FloorId");
            return View();
        }

        // POST: Admin/FloorEng/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EngId,EngName,BuildingId,FloorId,Rtp,Rtt")] FloorEngModel floorEngModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(floorEngModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EngId"] = new SelectList(_context.AppUsers, "Id", "Email", floorEngModel.EngId);
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "BuildingId", "BuildingId", floorEngModel.BuildingId);
            ViewData["BuildingId"] = new SelectList(_context.Floors, "BuildingId", "FloorId", floorEngModel.BuildingId);
            return View(floorEngModel);
        }

        // GET: Admin/FloorEng/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var floorEngModel = await _context.FloorEngs.SingleOrDefaultAsync(m => m.EngId == id);
            if (floorEngModel == null)
            {
                return NotFound();
            }
            ViewData["EngId"] = new SelectList(_context.AppUsers, "Id", "Email", floorEngModel.EngId);
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "BuildingId", "BuildingId", floorEngModel.BuildingId);
            ViewData["BuildingId"] = new SelectList(_context.Floors, "BuildingId", "FloorId", floorEngModel.BuildingId);
            return View(floorEngModel);
        }

        // POST: Admin/FloorEng/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EngId,EngName,BuildingId,FloorId,Rtp,Rtt")] FloorEngModel floorEngModel)
        {
            if (id != floorEngModel.EngId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(floorEngModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FloorEngModelExists(floorEngModel.EngId))
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
            ViewData["EngId"] = new SelectList(_context.AppUsers, "Id", "Email", floorEngModel.EngId);
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "BuildingId", "BuildingId", floorEngModel.BuildingId);
            ViewData["BuildingId"] = new SelectList(_context.Floors, "BuildingId", "FloorId", floorEngModel.BuildingId);
            return View(floorEngModel);
        }

        // GET: Admin/FloorEng/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var floorEngModel = await _context.FloorEngs
                .Include(f => f.AppUsers)
                .Include(f => f.Buildings)
                .Include(f => f.Floors)
                .SingleOrDefaultAsync(m => m.EngId == id);
            if (floorEngModel == null)
            {
                return NotFound();
            }

            return View(floorEngModel);
        }

        // POST: Admin/FloorEng/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var floorEngModel = await _context.FloorEngs.SingleOrDefaultAsync(m => m.EngId == id);
            _context.FloorEngs.Remove(floorEngModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FloorEngModelExists(int id)
        {
            return _context.FloorEngs.Any(e => e.EngId == id);
        }
    }
}
