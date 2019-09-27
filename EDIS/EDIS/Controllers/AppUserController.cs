using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Models;
using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Repositories;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EDIS.Controllers
{
    [Authorize]
    public class AppUserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly IRepository<DepartmentModel, string> _dptRepo;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;

        public AppUserController(ApplicationDbContext context,
                                 IRepository<AppUserModel, int> userRepo,
                                 IRepository<DepartmentModel, string> dptRepo,
                                 CustomUserManager customUserManager,
                                 CustomRoleManager customRoleManager)
        {
            _context = context;
            _userRepo = userRepo;
            _dptRepo = dptRepo;
            userManager = customUserManager;
            roleManager = customRoleManager;
        }

        // Get: AppUser/Index
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetUsersInDpt(string id)
        {
            DepartmentModel d = _context.Departments.Find(id);
            List<AppUserModel> ul;
            List<UserList> us = new List<UserList>();
            string s = "";
            if (d != null)
            {
                ul = _context.AppUsers.Where(p => p.DptId == d.DptId)
                                      .Where(p => p.Status == "Y").ToList();
                //s += "[";
                foreach (AppUserModel f in ul)
                {
                    UserList u = new UserList();
                    u.uid = f.Id;
                    u.uname = f.FullName;
                    us.Add(u);
                }
                s = JsonConvert.SerializeObject(us);
            }
            return Json(s);
        }

        public JsonResult GetUsersByKeyname(string keyname)
        {
            List<AppUserModel> ul;
            List<UserList> us = new List<UserList>();
            string s = "";
            string uid = keyname.PadLeft(10, '0');
            ul = _context.AppUsers.Where(p => p.UserName == uid)
                                  .Where(p => p.Status == "Y")
                                  .Union(_context.AppUsers.Where(p => p.FullName.Contains(keyname)))
                                  .ToList();
            foreach (AppUserModel f in ul)
            {
                UserList u = new UserList();
                u.uid = f.Id;
                u.uname = "(" + f.UserName + ")" + f.FullName;
                us.Add(u);
            }
            s = JsonConvert.SerializeObject(us);
            return Json(s);
        }

        public JsonResult GetUsersInDptByKeyname(string keyname)
        {
            List<AppUserModel> ul;
            List<UserList> us = new List<UserList>();
            string s = "";
            if (!string.IsNullOrEmpty(keyname))
            {
                _context.Departments.Where(u => u.Name_C.Contains(keyname))
                    .ToList()
                    .ForEach(d =>
                    {
                        ul = _context.AppUsers.Where(p => p.DptId == d.DptId)
                                              .Where(p => p.Status == "Y").ToList();
                        foreach (AppUserModel f in ul)
                        {
                            UserList u = new UserList();
                            u.uid = f.Id;
                            u.uname = "(" + f.UserName + ")" + f.FullName;
                            us.Add(u);
                        }
                        s = JsonConvert.SerializeObject(us);
                    });
            }
            s = JsonConvert.SerializeObject(us);

            return Json(s);
        }

        public string GetFullName()
        {
            return _context.AppUsers.Where(a => a.UserName == User.Identity.Name).FirstOrDefault().FullName;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class UserList
    {
        public int uid;
        public string uname;
    }

}