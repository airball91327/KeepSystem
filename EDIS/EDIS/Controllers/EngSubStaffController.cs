using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Data;
using EDIS.Models.RepairModels;
using EDIS.Models.Identity;
using Microsoft.AspNetCore.Authorization;

namespace EDIS.Controllers
{
    [Authorize]
    public class EngSubStaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CustomRoleManager roleManager;

        public EngSubStaffController(ApplicationDbContext context, CustomRoleManager customRoleManager)
        {
            _context = context;
            roleManager = customRoleManager;
        }

        // GET: EngSubStaff
        public async Task<IActionResult> Index()
        {
            var user = _context.AppUsers.Where(a => a.UserName == User.Identity.Name).First();
            var applicationDbContext = _context.EngSubStaff.Include(e => e.EngAppUsers).Include(e => e.SubAppUsers)
                                                           .Where(e => e.EngId == user.Id);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: EngSubStaff/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var engSubStaff = await _context.EngSubStaff
                .Include(e => e.EngAppUsers)
                .Include(e => e.SubAppUsers)
                .SingleOrDefaultAsync(m => m.EngId == id);
            if (engSubStaff == null)
            {
                return NotFound();
            }

            return View(engSubStaff);
        }

        // GET: EngSubStaff/Create
        public IActionResult Create()
        {
            var user = _context.AppUsers.Where(a => a.UserName == User.Identity.Name).First();
            /* Get all engineers by role. */
            var allEngs = roleManager.GetUsersInRole("RepEngineer").ToList();
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem li = new SelectListItem();
            foreach (string l in allEngs)
            {
                var u = _context.AppUsers.Where(a => a.UserName == l).FirstOrDefault();
                if (u != null)
                {
                    li = new SelectListItem();
                    li.Text = u.FullName;
                    li.Value = u.Id.ToString();
                    list.Add(li);
                }
            }

            ViewData["EngId"] = user.Id;
            ViewData["SubstituteId"] = new SelectList(list, "Value", "Text");
            return View();
        }

        // POST: EngSubStaff/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EngId,SubstituteId,StartDate,EndDate")] EngSubStaff engSubStaff)
        {
            if (ModelState.IsValid)
            {
                _context.Add(engSubStaff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var user = _context.AppUsers.Where(a => a.UserName == User.Identity.Name).First();
            /* Get all engineers by role. */
            var allEngs = roleManager.GetUsersInRole("RepEngineer").ToList();
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem li = new SelectListItem();
            foreach (string l in allEngs)
            {
                var u = _context.AppUsers.Where(a => a.UserName == l).FirstOrDefault();
                if (u != null)
                {
                    li = new SelectListItem();
                    li.Text = u.FullName;
                    li.Value = u.Id.ToString();
                    list.Add(li);
                }
            }
            ViewData["EngId"] = user.Id;
            ViewData["SubstituteId"] = new SelectList(list, "Value", "Text");
            return View(engSubStaff);
        }

        // GET: EngSubStaff/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var engSubStaff = await _context.EngSubStaff.SingleOrDefaultAsync(m => m.EngId == id);
            if (engSubStaff == null)
            {
                return NotFound();
            }
            /* Get all engineers by role. */
            var allEngs = roleManager.GetUsersInRole("RepEngineer").ToList();
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem li = new SelectListItem();
            foreach (string l in allEngs)
            {
                var u = _context.AppUsers.Where(a => a.UserName == l).FirstOrDefault();
                if (u != null)
                {
                    li = new SelectListItem();
                    li.Text = u.FullName;
                    li.Value = u.Id.ToString();
                    list.Add(li);
                }
            }
            ViewData["SubstituteId"] = new SelectList(list, "Value", "Text");
            return View(engSubStaff);
        }

        // POST: EngSubStaff/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EngId,SubstituteId,StartDate,EndDate")] EngSubStaff engSubStaff)
        {
            if (id != engSubStaff.EngId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(engSubStaff);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EngSubStaffExists(engSubStaff.EngId))
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
            /* Get all engineers by role. */
            var allEngs = roleManager.GetUsersInRole("RepEngineer").ToList();
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem li = new SelectListItem();
            foreach (string l in allEngs)
            {
                var u = _context.AppUsers.Where(a => a.UserName == l).FirstOrDefault();
                if (u != null)
                {
                    li = new SelectListItem();
                    li.Text = u.FullName;
                    li.Value = u.Id.ToString();
                    list.Add(li);
                }
            }
            ViewData["SubstituteId"] = new SelectList(list, "Value", "Text");
            return View(engSubStaff);
        }

        // GET: EngSubStaff/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var engSubStaff = await _context.EngSubStaff
                .Include(e => e.EngAppUsers)
                .Include(e => e.SubAppUsers)
                .SingleOrDefaultAsync(m => m.EngId == id);
            if (engSubStaff == null)
            {
                return NotFound();
            }

            return View(engSubStaff);
        }

        // POST: EngSubStaff/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var engSubStaff = await _context.EngSubStaff.SingleOrDefaultAsync(m => m.EngId == id);
            _context.EngSubStaff.Remove(engSubStaff);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EngSubStaffExists(int id)
        {
            return _context.EngSubStaff.Any(e => e.EngId == id);
        }
    }
}
