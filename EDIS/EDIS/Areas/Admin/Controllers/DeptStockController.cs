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
    public class DeptStockController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public DeptStockController(ApplicationDbContext context,
                                   IRepository<AppUserModel, int> userRepo,
                                   CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        // GET: Admin/DeptStock
        public async Task<IActionResult> Index(int? stockClsId, int? stockItemId)
        {
            ViewBag.StockClsId = new SelectList(_context.DeptStockClasses, "StockClsId", "StockClsName", stockClsId);
            ViewBag.StockItemId = stockItemId;

            var applicationDbContext = _context.DeptStocks.Include(d => d.DeptStockClass).Include(d => d.DeptStockItem);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/DeptStock/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deptStockModel = await _context.DeptStocks
                .Include(d => d.DeptStockClass)
                .Include(d => d.DeptStockItem)
                .SingleOrDefaultAsync(m => m.StockId == id);
            if (deptStockModel == null)
            {
                return NotFound();
            }

            return View(deptStockModel);
        }

        // GET: Admin/DeptStock/Create
        public IActionResult Create(int stockClsId, int stockItemId)
        {
            ViewBag.StockClsName = _context.DeptStockClasses.Find(stockClsId).StockClsName;
            ViewBag.StockItemName = _context.DeptStockItems.Find(stockClsId, stockItemId).StockItemName;

            /* Set default value. */
            DeptStockModel deptStockModel = new DeptStockModel
            {
                StockClsId = stockClsId,
                StockItemId = stockItemId
            };
            return View(deptStockModel);
        }

        // POST: Admin/DeptStock/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StockId,StockClsId,StockItemId,StockNo,StockName,Unite,Price,Qty,SafeQty,Loc,Standard,Brand,Status,Rtp,Rtt,CustOrgan_CustId")] DeptStockModel deptStockModel)
        {
            ViewBag.StockClsName = _context.DeptStockClasses.Find(deptStockModel.StockClsId).StockClsName;
            ViewBag.StockItemName = _context.DeptStockItems.Find(deptStockModel.StockClsId, deptStockModel.StockItemId).StockItemName;
            
            // Get Login User's details.
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            // Set values.
            deptStockModel.Rtp = ur.Id;
            deptStockModel.Rtt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(deptStockModel);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { stockClsId = deptStockModel.StockClsId, stockItemId = deptStockModel.StockItemId, area = "Admin" });
            }
            return View(deptStockModel);
        }

        // GET: Admin/DeptStock/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deptStockModel = await _context.DeptStocks
                .Include(d => d.DeptStockClass)
                .Include(d => d.DeptStockItem)
                .SingleOrDefaultAsync(m => m.StockId == id);

            if (deptStockModel == null)
            {
                return NotFound();
            }
            return View(deptStockModel);
        }

        // POST: Admin/DeptStock/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StockId,StockClsId,StockItemId,StockNo,StockName,Unite,Price,Qty,SafeQty,Loc,Standard,Brand,Status,Rtp,Rtt,CustOrgan_CustId")] DeptStockModel deptStockModel)
        {
            // Get Login User's details.
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            // Set values.
            deptStockModel.Rtp = ur.Id;
            deptStockModel.Rtt = DateTime.Now;

            if (id != deptStockModel.StockId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deptStockModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeptStockModelExists(deptStockModel.StockId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", new { stockClsId = deptStockModel.StockClsId, stockItemId = deptStockModel.StockItemId, area = "Admin" });
            }
            return View(deptStockModel);
        }

        // GET: Admin/DeptStock/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deptStockModel = await _context.DeptStocks
                .Include(d => d.DeptStockClass)
                .Include(d => d.DeptStockItem)
                .SingleOrDefaultAsync(m => m.StockId == id);
            if (deptStockModel == null)
            {
                return NotFound();
            }

            return View(deptStockModel);
        }

        // POST: Admin/DeptStock/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deptStockModel = await _context.DeptStocks.SingleOrDefaultAsync(m => m.StockId == id);
            _context.DeptStocks.Remove(deptStockModel);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { stockClsId = deptStockModel.StockClsId, stockItemId = deptStockModel.StockItemId, area = "Admin" });
        }

        // GET: Admin/DeptStock/GetItems/5
        public JsonResult GetItems(int stockClsId)
        {
            var itemsOfClass = _context.DeptStockItems.Where(d => d.StockClsId == stockClsId);
            var items = itemsOfClass.Select(i => new
            {
                stockItemId = i.StockItemId,
                stockItemName = i.StockItemName
            });
            return Json(items);
        }

        // GET: Admin/DeptStock/GetStockList/5
        public IActionResult GetStockList(int? stockClsId, int? stockItemId)
        {
            ViewBag.StockClsName = _context.DeptStockClasses.Find(stockClsId).StockClsName;
            ViewBag.StockClsId = stockClsId;
            ViewBag.StockItemName = _context.DeptStockItems.Find(stockClsId, stockItemId).StockItemName;
            ViewBag.StockItemId = stockItemId;

            var stockList = _context.DeptStocks.Where(d => d.StockClsId == stockClsId &&
                                                           d.StockItemId == stockItemId);
            /* Get user account to show. */
            foreach (var item in stockList)
            {
                item.RtpName = _context.AppUsers.Find(item.Rtp).UserName;
            }
            return View(stockList.ToList());
        }

        private bool DeptStockModelExists(int id)
        {
            return _context.DeptStocks.Any(e => e.StockId == id);
        }
    }
}
