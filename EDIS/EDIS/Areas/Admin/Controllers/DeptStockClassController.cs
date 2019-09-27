using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Data;
using EDIS.Models.RepairModels;
using EDIS.Repositories;
using EDIS.Models.Identity;
using Microsoft.AspNetCore.Authorization;

namespace EDIS.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize]
    public class DeptStockClassController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public DeptStockClassController(ApplicationDbContext context,
                                        IRepository<AppUserModel, int> userRepo,
                                        CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        // GET: Admin/DeptStockClass
        public async Task<IActionResult> Index()
        {
            var stockClassList = _context.DeptStockClasses;
            /* Get user account to show. */
            foreach (var item in stockClassList)
            {
                item.RtpName = _context.AppUsers.Find(item.Rtp).UserName;
            }
            return View(await _context.DeptStockClasses.ToListAsync());
        }

        // GET: Admin/DeptStockClass/Create
        public IActionResult Create()
        {
            DeptStockClassModel deptStockClassModel = new DeptStockClassModel()
            {
                Flg = "Y"
            };
            return View(deptStockClassModel);
        }

        // POST: Admin/DeptStockClass/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StockClsId,StockClsName,Flg,Rtp,Rtt")] DeptStockClassModel deptStockClassModel)
        {
            // Get Login User's details.
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            deptStockClassModel.Rtp = ur.Id;
            deptStockClassModel.Rtt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(deptStockClassModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(deptStockClassModel);
        }

        // GET: Admin/DeptStockClass/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deptStockClassModel = await _context.DeptStockClasses.SingleOrDefaultAsync(m => m.StockClsId == id);
            if (deptStockClassModel == null)
            {
                return NotFound();
            }
            return View(deptStockClassModel);
        }

        // POST: Admin/DeptStockClass/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StockClsId,StockClsName,Flg,Rtp,Rtt")] DeptStockClassModel deptStockClassModel)
        {
            // Get Login User's details.
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            deptStockClassModel.Rtp = ur.Id;
            deptStockClassModel.Rtt = DateTime.Now;

            if (id != deptStockClassModel.StockClsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deptStockClassModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeptStockClassModelExists(deptStockClassModel.StockClsId))
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
            return View(deptStockClassModel);
        }

        private bool DeptStockClassModelExists(int id)
        {
            return _context.DeptStockClasses.Any(e => e.StockClsId == id);
        }
    }
}
