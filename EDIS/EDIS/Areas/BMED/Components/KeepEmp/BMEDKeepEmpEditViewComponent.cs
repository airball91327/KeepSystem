using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Components.KeepEmp
{
    public class BMEDKeepEmpEditViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;

        public BMEDKeepEmpEditViewComponent(BMEDDbContext context,
                                            IRepository<AppUserModel, int> userRepo,
                                            CustomUserManager customUserManager,
                                            CustomRoleManager customRoleManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
            roleManager = customRoleManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string docId)
        {
            var keepEmps = _context.BMEDKeepEmps.ToList();
            KeepEmpModel emp = keepEmps.Where(p => p.DocId == docId).FirstOrDefault();
            var ur = _userRepo.Find(us => us.UserName == this.User.Identity.Name).FirstOrDefault();

            if (emp == null)
            {
                emp = new KeepEmpModel();
                emp.DocId = docId;
                emp.UserId = ur.Id;
            }

            /* Get all engineers by role. */
            var allEngs = roleManager.GetUsersInRole("MedEngineer").ToList();
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
            ViewData["EmpList"] = new SelectList(list, "Value", "Text");

            KeepFlowModel rf = _context.BMEDKeepFlows.Where(f => f.DocId == docId)
                                                     .Where(f => f.Status == "?").FirstOrDefault();
            var isEngineer = _context.UsersInRoles.Where(u => u.AppRoles.RoleName == "MedEngineer" &&
                                                              u.UserId == ur.Id).FirstOrDefault();
            if (!(rf.Cls.Contains("工程師") && rf.UserId == ur.Id))    /* 流程 => 其他 */
            {
                if (rf.Cls.Contains("工程師") && isEngineer != null)   /* 流程 => 工程師，Login User => 非負責之工程師 */
                {
                    return View(emp);
                }
                AppUserModel appuser;
                List<KeepEmpModel> emps = keepEmps.Where(p => p.DocId == docId).ToList();
                emps.ForEach(rp =>
                {
                    rp.UserName = (appuser = _context.AppUsers.Find(rp.UserId)) == null ? "" : appuser.UserName;
                    rp.FullName = (appuser = _context.AppUsers.Find(rp.UserId)) == null ? "" : appuser.FullName;
                });
                return View("Details", emps);
            }
            /* 流程 => 工程師，Login User => 負責之工程師 */
            return View(emp);
        }

    }
}
