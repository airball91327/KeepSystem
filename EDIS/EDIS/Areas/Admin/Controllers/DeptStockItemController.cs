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
    public class DeptStockItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public DeptStockItemController(ApplicationDbContext context,
                                       IRepository<AppUserModel, int> userRepo,
                                       CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        // GET: Admin/DeptStockItem
        public async Task<IActionResult> Index(int? stockClsId)
        {
            ViewData["StockClsId"] = new SelectList(_context.DeptStockClasses, "StockClsId", "StockClsName", stockClsId);
            var applicationDbContext = _context.DeptStockItems.Include(d => d.DeptStockClass);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/DeptStockItem/GetStockItemList/5
        public IActionResult GetStockItemList(int? stockClsId)
        {
            ViewBag.StockClassName = _context.DeptStockClasses.Find(stockClsId).StockClsName;
            ViewBag.StockClsId = stockClsId;
            var stockItemList = _context.DeptStockItems.Where(d => d.StockClsId == stockClsId);

            /* Get user account to show. */
            foreach (var item in stockItemList)
            {
                item.RtpName = _context.AppUsers.Find(item.Rtp).UserName;
            }
            return View(stockItemList.ToList());
        }

        // GET: Admin/DeptStockItem/Create
        public IActionResult Create(int stockClsId)
        {
            ViewBag.StockClassName = _context.DeptStockClasses.Find(stockClsId).StockClsName;
            // Set default value.
            DeptStockItemModel deptStockItemModel = new DeptStockItemModel()
            {
                StockClsId = stockClsId,
                Flg = "Y"
            };
            return View(deptStockItemModel);
        }

        // POST: Admin/DeptStockItem/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int stockClsId, [Bind("StockClsId,StockItemId,StockItemName,Flg,Rtp,Rtt")] DeptStockItemModel deptStockItemModel)
        {
            ViewBag.StockClassName = _context.DeptStockClasses.Find(stockClsId).StockClsName;
            // Get Login User's details.
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            // Set values.
            deptStockItemModel.Rtp = ur.Id;
            deptStockItemModel.Rtt = DateTime.Now;

            /* Set item id. */
            var lastItem = _context.DeptStockItems.Where(d => d.StockClsId == stockClsId);
            if (lastItem.Count() == 0)
            {
                // If the class hasn't any item.
                var lastItemId = 1;
                deptStockItemModel.StockItemId = lastItemId;
            }
            else
            {
                var lastItemId = lastItem.OrderByDescending(i => i.StockItemId).First().StockItemId;
                deptStockItemModel.StockItemId = lastItemId + 1;
            }

            if (ModelState.IsValid)
            {
                _context.Add(deptStockItemModel);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { StockClsId = stockClsId });
            }
            return View(deptStockItemModel);
        }

        // GET: Admin/DeptStockItem/Edit/5
        public async Task<IActionResult> Edit(int? stockClsId, int? stockItemId)
        {
            ViewBag.StockClassName = _context.DeptStockClasses.Find(stockClsId).StockClsName;
            if (stockClsId == null || stockItemId == null)
            {
                return NotFound();
            }

            var deptStockItemModel = await _context.DeptStockItems.FindAsync(stockClsId, stockItemId);
            if (deptStockItemModel == null)
            {
                return NotFound();
            }
            return View(deptStockItemModel);
        }

        // POST: Admin/DeptStockItem/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int stockClsId, int stockItemId, [Bind("StockClsId,StockItemId,StockItemName,Flg,Rtp,Rtt")] DeptStockItemModel deptStockItemModel)
        {
            ViewBag.StockClassName = _context.DeptStockClasses.Find(stockClsId).StockClsName;
            // Get Login User's details.
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            deptStockItemModel.Rtp = ur.Id;
            deptStockItemModel.Rtt = DateTime.Now;

            if (stockClsId != deptStockItemModel.StockClsId || stockItemId != deptStockItemModel.StockItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deptStockItemModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeptStockItemModelExists(deptStockItemModel.StockClsId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", new { StockClsId = stockClsId });
            }
            return View(deptStockItemModel);
        }

        private bool DeptStockItemModelExists(int id)
        {
            return _context.DeptStockItems.Any(e => e.StockClsId == id);
        }
    }
}
