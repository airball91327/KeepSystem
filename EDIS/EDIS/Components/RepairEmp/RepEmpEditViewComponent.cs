using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.RepairModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.RepairEmp
{
    public class RepEmpEditViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;

        public RepEmpEditViewComponent(ApplicationDbContext context,
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
            var repairEmps = _context.RepairEmps.ToList();
            RepairEmpModel emp = repairEmps.Where(p => p.DocId == docId)
                                           .FirstOrDefault();
            var ur = _userRepo.Find(us => us.UserName == this.User.Identity.Name).FirstOrDefault();
            if (emp == null)
            {
                emp = new RepairEmpModel();
                emp.DocId = docId;
                emp.UserId = ur.Id;
            }

            /* Get all engineers. */
            //var engineerList = _context.EngsInDepts.Include(e => e.AppUsers).Include(e => e.Departments)
            //                                       .GroupBy(e => e.EngId).Select(group => group.First()).ToList();
            //List<SelectListItem> uids = new List<SelectListItem>();
            //foreach (var item in engineerList)
            //{
            //    uids.Add(new SelectListItem
            //    {                   
            //        Text = item.AppUsers.FullName,
            //        Value = item.AppUsers.Id.ToString(),
            //    });
            //}
            //ViewData["EmpList"] = new SelectList(uids, "Value", "Text", ur.Id);
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
            ViewData["EmpList"] = new SelectList(list, "Value", "Text");

            RepairFlowModel rf = _context.RepairFlows.Where(f => f.DocId == docId)
                                                     .Where(f => f.Status == "?").FirstOrDefault();
            var isEngineer = _context.UsersInRoles.Where(u => u.AppRoles.RoleName == "RepEngineer" &&
                                                              u.UserId == ur.Id).FirstOrDefault();
            /* 查無處理中流程 => 已結案 或 已廢除 */
            if (rf == null)
            {
                /* Role => 工務經辦 or Admin */
                if (userManager.IsInRole(this.UserClaimsPrincipal, "RepToDo") == true ||
                    userManager.IsInRole(this.UserClaimsPrincipal, "Admin") == true ||
                    userManager.IsInRole(this.UserClaimsPrincipal, "RepAdmin"))
                {
                    return View(emp);
                }
            }
            if (!(rf.Cls.Contains("工程師") && rf.UserId == ur.Id))    /* 流程 => 其他 */
            {
                if (rf.Cls.Contains("工程師") && isEngineer != null)   /* 流程 => 工程師，Login User => 非負責之工程師 */
                {
                    return View(emp);
                }
                AppUserModel appuser;
                List<RepairEmpModel> emps = repairEmps.Where(p => p.DocId == docId).ToList();
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
