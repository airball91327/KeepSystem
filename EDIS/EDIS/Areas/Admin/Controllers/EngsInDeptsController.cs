using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Data;
using EDIS.Models.RepairModels;
using Microsoft.AspNetCore.Authorization;
using EDIS.Repositories;
using EDIS.Models.Identity;

namespace EDIS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class EngsInDeptsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;

        public EngsInDeptsController(ApplicationDbContext context,
                                     IRepository<AppUserModel, int> userRepo,
                                     CustomUserManager customUserManager,
                                     CustomRoleManager customRoleManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
            roleManager = customRoleManager;
        }

        // GET: Admin/EngsInDepts
        public async Task<IActionResult> Index()
        {
            /* Get all engineers */
            var repEngs = roleManager.GetUsersInRole("RepEngineer").ToList();
            /* Insert engineer list into dropdownlist. */
            List<SelectListItem> engineerList = new List<SelectListItem>();
            foreach (string uName in repEngs)
            {
                AppUserModel u = _context.AppUsers.Where(ur => ur.UserName == uName).FirstOrDefault();
                if(u != null)
                {
                    engineerList.Add(new SelectListItem()
                    {
                        Text = u.FullName + "(" + u.UserName + ")",
                        Value = u.Id.ToString()
                    });
                }             
            }
            var buildings = _context.Buildings.ToList();
            /* Insert building list into dropdownlist. */
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

            return View();
        }

        // GET: Admin/EngsInDepts/GetEngList
        public async Task<IActionResult> GetEngList(string EngineerId, string BuildingId, string FloorId)
        {
            /* Left Outer Join table "Places" and "EngsInDepts". */
            var query = from p in _context.Places
                        join eng in _context.EngsInDepts
                        on
                            new { p.BuildingId, p.FloorId, p.PlaceId } // Join with multiple fields.
                        equals
                            new { eng.BuildingId, eng.FloorId, eng.PlaceId } into subGrp
                        from s in subGrp.DefaultIfEmpty()
                        select new
                        {
                            EngId = (s == null ? 0 : s.EngId),                  // Outer Join need to deal "null" type
                            UserName = (s == null ? "N/A" : s.UserName),
                            FullName = (s == null ? "N/A" : s.AppUsers.FullName),
                            //Rtp = (s == null ? 0 : s.Rtp),
                            //Rtt = (s == null ? null : s.Rtt),
                            p.BuildingId,
                            p.FloorId,
                            p.PlaceId,
                            p.PlaceName
                        };
            var engList = query.ToList();

            /* Query "BuildingId". */
            if (BuildingId != null)
            {
                engList = engList.Where(e => e.BuildingId == Convert.ToInt32(BuildingId)).ToList();
            }
            /* Query "FloorId". */
            if (FloorId != null)
            {
                engList = engList.Where(e => e.FloorId == FloorId).ToList();
            }
            /* Query "EngineerId". */
            if (EngineerId != null)
            {
                engList = engList.Where(e => e.EngId == Convert.ToInt32(EngineerId)).ToList();
            }

            /* Insert query result into engsInDeptsVModel to display. */
            List<EngsInDeptsViewModel> engsInDeptsList = new List<EngsInDeptsViewModel>();
            foreach (var item in engList)
            {
                int? engId = item.EngId;
                //int? rtp = item.Rtp;
                //string userName = "N/A";
                if (item.EngId == 0)    //If EngId is not found.
                {
                    engId = null;
                }
                //if (item.Rtp == 0)       //If Rtp is not found.
                //{
                //    rtp = null;
                //}
                //if (_context.AppUsers.Find(item.Rtp) != null)    //If Rtp is not null.
                //{
                //    userName = _context.AppUsers.Find(item.Rtp).UserName;
                //}
                engsInDeptsList.Add(new EngsInDeptsViewModel()
                {
                    IsSelected = false,
                    BuildingId = item.BuildingId,
                    BuildingName = _context.Buildings.Find(item.BuildingId).BuildingName,
                    FloorId = item.FloorId,
                    FloorName = _context.Floors.Find(item.BuildingId, item.FloorId).FloorName,
                    PlaceId = item.PlaceId,
                    PlaceName = item.PlaceName,
                    EngId = engId,
                    UserName = item.UserName,
                    EngFullName = item.FullName,
                    //Rtp = rtp,
                    //RtpName = userName,
                    //Rtt = item.Rtt
                });
            }
            /* Order list by BuildingId then FloorId and PlaceId.*/
            engsInDeptsList = engsInDeptsList.OrderBy(f => f.BuildingId).ThenBy(f => Convert.ToInt32(f.FloorId)).ToList();

            /* Get all engineers */
            var repEngs = roleManager.GetUsersInRole("RepEngineer").ToList();
            /* Insert engineer list into dropdownlist. */
            List<SelectListItem> engineerList = new List<SelectListItem>();
            foreach (string uName in repEngs)
            {
                AppUserModel u = _context.AppUsers.Where(ur => ur.UserName == uName).FirstOrDefault();
                if (u != null)
                {
                    engineerList.Add(new SelectListItem()
                    {
                        Text = u.FullName + "(" + u.UserName + ")",
                        Value = u.Id.ToString()
                    });
                }
            }
            ViewData["AsignEngId"] = engineerList;
            ViewData["QueryEngId"] = EngineerId;
            ViewData["QueryBuildingId"] = BuildingId;
            ViewData["QueryFloorId"] = FloorId;

            return View("EditList", engsInDeptsList);
        }

        // POST: Admin/EngsInDepts/EditEngList
        [HttpPost]
        public async Task<IActionResult> EditEngList(List<EngsInDeptsViewModel> data, string AsignEngId,
                                                     string QueryEngId, string QueryBuildingId, string QueryFloorId)
        {
            // Get Login User's details.
            var user = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            /* Target engineer ID. */
            int asignEngId = Convert.ToInt32(AsignEngId);

            try
            {
                /* Deal all input data. */
                foreach (var item in data)
                {
                    /* Create or edit the selected row data. */
                    if (item.IsSelected == true)
                    {
                        /* Insert values to engsInDeptsModel to save or create. */
                        EngsInDeptsModel engsInDeptsModel = new EngsInDeptsModel
                        {
                            EngId = asignEngId,
                            BuildingId = item.BuildingId,
                            FloorId = item.FloorId,
                            PlaceId = item.PlaceId,
                            DptId = _context.AppUsers.Find(asignEngId).DptId,
                            UserName = _context.AppUsers.Find(asignEngId).UserName
                            //Rtp = user.Id,
                            //Rtt = DateTime.UtcNow.AddHours(08)
                        };
                        /* If data isn't in the database, create data.*/
                        if (item.EngId == null)
                        {
                            _context.Add(engsInDeptsModel);
                        }
                        else
                        {
                            /* If data exist, find and delete old data, then create a new one. */
                            int engId = item.EngId.GetValueOrDefault();
                            var originData = _context.EngsInDepts.Find(engId, item.BuildingId, item.FloorId, item.PlaceId);
                            if (originData.EngId != asignEngId)
                            {
                                _context.Remove(originData);
                                _context.Add(engsInDeptsModel);
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                var msg = "資料更新錯誤!";
                return StatusCode(500, msg);
            }
            return RedirectToAction("GetEngList", new { EngineerId = QueryEngId, BuildingId = QueryBuildingId, FloorId = QueryFloorId });
        }

        // GET: Admin/EngsInDept
        public async Task<IActionResult> EngsInDept()
        {
            /* Get groups and set department dropdown list. */
            var departments = _context.EngsInDepts.Include(e => e.Departments)
                                      .GroupBy(e => e.DptId).Select(group => group.First()).ToList() ;
            List<SelectListItem> deptList = new List<SelectListItem>();
            foreach (var item in departments)
            {
                deptList.Add(new SelectListItem()
                {
                    Text = item.DptId + item.Departments.Name_C,
                    Value = item.DptId
                });
            }

            ViewData["DptId"] = deptList;
            return View();
        }

        // GET: Admin/EngsInDepts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var engsInDeptsModel = await _context.EngsInDepts
                .Include(e => e.AppUsers)
                .Include(e => e.Departments)
                .SingleOrDefaultAsync(m => m.EngId == id);
            if (engsInDeptsModel == null)
            {
                return NotFound();
            }

            return View(engsInDeptsModel);
        }

        // GET: Admin/EngsInDepts/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.AppUsers, "Id", "Email");
            ViewData["DptId"] = new SelectList(_context.Departments, "DptId", "DptId");
            return View();
        }

        // POST: Admin/EngsInDepts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DptId,UserName")] EngsInDeptsModel engsInDeptsModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(engsInDeptsModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.AppUsers, "Id", "Email", engsInDeptsModel.EngId);
            ViewData["DptId"] = new SelectList(_context.Departments, "DptId", "DptId", engsInDeptsModel.DptId);
            return View(engsInDeptsModel);
        }

        // GET: Admin/EngsInDepts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var engsInDeptsModel = await _context.EngsInDepts.SingleOrDefaultAsync(m => m.EngId == id);
            if (engsInDeptsModel == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.AppUsers, "Id", "Email", engsInDeptsModel.EngId);
            ViewData["DptId"] = new SelectList(_context.Departments, "DptId", "DptId", engsInDeptsModel.DptId);
            return View(engsInDeptsModel);
        }

        // POST: Admin/EngsInDepts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DptId,UserName")] EngsInDeptsModel engsInDeptsModel)
        {
            if (id != engsInDeptsModel.EngId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(engsInDeptsModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EngsInDeptsModelExists(engsInDeptsModel.EngId))
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
            ViewData["Id"] = new SelectList(_context.AppUsers, "Id", "Email", engsInDeptsModel.EngId);
            ViewData["DptId"] = new SelectList(_context.Departments, "DptId", "DptId", engsInDeptsModel.DptId);
            return View(engsInDeptsModel);
        }

        // GET: Admin/EngsInDepts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var engsInDeptsModel = await _context.EngsInDepts
                .Include(e => e.AppUsers)
                .Include(e => e.Departments)
                .SingleOrDefaultAsync(m => m.EngId == id);
            if (engsInDeptsModel == null)
            {
                return NotFound();
            }

            return View(engsInDeptsModel);
        }

        // POST: Admin/EngsInDepts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var engsInDeptsModel = await _context.EngsInDepts.SingleOrDefaultAsync(m => m.EngId == id);
            _context.EngsInDepts.Remove(engsInDeptsModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EngsInDeptsModelExists(int id)
        {
            return _context.EngsInDepts.Any(e => e.EngId == id);
        }
    }
}
