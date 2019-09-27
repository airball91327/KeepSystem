using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Data;
using EDIS.Models.Identity;
using Microsoft.AspNetCore.Authorization;

namespace EDIS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ExternalUserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExternalUserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ExternalUser
        public async Task<IActionResult> Index()
        {
            var externalUsers = await _context.ExternalUsers.Include(e => e.AppUsers).ToListAsync();
            return View(externalUsers);
        }

        // GET: Admin/ExternalUser/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var externalUserModel = await _context.ExternalUsers.Include(e => e.AppUsers)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (externalUserModel == null)
            {
                return NotFound();
            }

            return View(externalUserModel);
        }

        // GET: Admin/ExternalUser/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ExternalUser/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserName,FullName,Password,Email,Ext,Mobile,DptId,VendorId,DateCreated,LastActivityDate,Status")] AppUserModel appUserModel)
        {
            if (ModelState.IsValid)
            {
                /* Check userName. */
                var nameIsExist = _context.AppUsers.Where(u => u.UserName == appUserModel.UserName).FirstOrDefault();
                if(nameIsExist != null)
                {
                    ModelState.AddModelError("UserName", "此ID已被使用");
                    return View(appUserModel);
                }

                /* Create user to appUser table. */
                appUserModel.DateCreated = DateTime.Now;
                _context.Add(appUserModel);
                /* Create same data to externalUsers table. */
                ExternalUserModel externalUserModel = new ExternalUserModel()
                {
                    Id = appUserModel.Id,
                    Password = appUserModel.Password,
                    UserName = appUserModel.UserName
                };
                _context.Add(externalUserModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(appUserModel);
        }

        // GET: Admin/ExternalUser/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUserModel = await _context.AppUsers.SingleOrDefaultAsync(m => m.Id == id);
            if (appUserModel == null)
            {
                return NotFound();
            }
            return View(appUserModel);
        }

        // POST: Admin/ExternalUser/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,FullName,Password,Email,Ext,Mobile,DptId,VendorId,DateCreated,LastActivityDate,Status")] AppUserModel appUserModel)
        {
            if (id != appUserModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    /* Update appuser. */
                    appUserModel.LastActivityDate = DateTime.Now;
                    _context.Update(appUserModel);
                    /* Update externalUser. */
                    var externalUserModel = _context.ExternalUsers.Find(appUserModel.Id);
                    externalUserModel.Password = appUserModel.Password;
                    _context.Update(externalUserModel);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppUserModelExists(appUserModel.Id))
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
            return View(appUserModel);
        }

        // GET: Admin/ExternalUser/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUserModel = await _context.AppUsers
                .SingleOrDefaultAsync(m => m.Id == id);
            if (appUserModel == null)
            {
                return NotFound();
            }

            return View(appUserModel);
        }

        // POST: Admin/ExternalUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appUserModel = await _context.AppUsers.SingleOrDefaultAsync(m => m.Id == id);
            var externalUserModel = await _context.ExternalUsers.SingleOrDefaultAsync(m => m.Id == id);
            _context.ExternalUsers.Remove(externalUserModel);
            _context.AppUsers.Remove(appUserModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppUserModelExists(int id)
        {
            return _context.AppUsers.Any(e => e.Id == id);
        }
    }
}
