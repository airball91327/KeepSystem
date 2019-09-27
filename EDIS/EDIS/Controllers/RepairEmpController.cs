using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Data;
using EDIS.Models;
using EDIS.Models.Identity;
using EDIS.Models.LocationModels;
using EDIS.Models.RepairModels;
using EDIS.Repositories;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Controllers
{
    [Authorize]
    public class RepairEmpController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<RepairModel, string> _repRepo;
        private readonly IRepository<RepairDtlModel, string> _repdtlRepo;
        private readonly IRepository<RepairFlowModel, string[]> _repflowRepo;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly IRepository<DepartmentModel, string> _dptRepo;
        private readonly IRepository<DocIdStore, string[]> _dsRepo;
        private readonly IRepository<BuildingModel, int> _buildRepo;
        private readonly IRepository<RepairEmpModel, string[]> _repempRepo;
        private readonly IEmailSender _emailSender;
        private readonly CustomUserManager userManager;

        public RepairEmpController(ApplicationDbContext context,
                                   IRepository<RepairModel, string> repairRepo,
                                   IRepository<RepairDtlModel, string> repairdtlRepo,
                                   IRepository<RepairFlowModel, string[]> repairflowRepo,
                                   IRepository<AppUserModel, int> userRepo,
                                   IRepository<DepartmentModel, string> dptRepo,
                                   IRepository<DocIdStore, string[]> dsRepo,
                                   IRepository<BuildingModel, int> buildRepo,
                                   IRepository<RepairEmpModel, string[]> repairempRepo,
                                   IEmailSender emailSender,
                                   CustomUserManager customUserManager)
        {
            _context = context;
            _repRepo = repairRepo;
            _repdtlRepo = repairdtlRepo;
            _repflowRepo = repairflowRepo;
            _userRepo = userRepo;
            _dptRepo = dptRepo;
            _dsRepo = dsRepo;
            _buildRepo = buildRepo;
            _repempRepo = repairempRepo;
            _emailSender = emailSender;
            userManager = customUserManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RepairEmpModel repairEmp)
        {
            if (ModelState.IsValid)
            {
                _context.RepairEmps.Add(repairEmp);
                _context.SaveChanges();
                // Recount the all repair time, and set value to RepairDtl.
                RepairDtlModel dtl = _context.RepairDtls.Where(d => d.DocId == repairEmp.DocId)
                   .FirstOrDefault();
                if (dtl != null)
                {
                    int hr = _context.RepairEmps.Where(p => p.DocId == repairEmp.DocId)
                                                .Select(p => p.Hour)
                                                .DefaultIfEmpty(0).Sum();
                    decimal min = _context.RepairEmps.Where(p => p.DocId == repairEmp.DocId)
                                                     .Select(p => p.Minute)
                                                     .DefaultIfEmpty(0).Sum();
                    dtl.Hour = hr + Decimal.Round(min / 60m, 2);
                    _context.RepairDtls.Update(dtl);
                }
                return RedirectToAction("Index");
            }

            return View(repairEmp);
        }

        [HttpPost]
        public ActionResult Edit(RepairEmpModel repairEmp)
        {
            if (ModelState.IsValid)
            {
                //_context.RepairEmps.Add(repairEmp);
                //_context.SaveChanges();
                var ExistRepairEmp = _context.RepairEmps.Find(repairEmp.DocId, repairEmp.UserId);
                if(ExistRepairEmp != null)
                {
                    return new JsonResult(repairEmp)
                    {
                        Value = new { isExist = true, error = "資料已存在!" }
                    };
                }
                else
                {
                    _repempRepo.Create(repairEmp);
                }

                // Recount the all repair time, and set value to RepairDtl.
                RepairDtlModel dtl = _context.RepairDtls.Where(d => d.DocId == repairEmp.DocId)
                                                        .FirstOrDefault();
                if (dtl != null)
                {
                    int hr = _context.RepairEmps.Where(p => p.DocId == repairEmp.DocId)
                                                .Select(p => p.Hour)
                                                .DefaultIfEmpty(0).Sum();
                    decimal min = _context.RepairEmps.Where(p => p.DocId == repairEmp.DocId)
                                                     .Select(p => p.Minute)
                                                     .DefaultIfEmpty(0).Sum();
                    dtl.Hour = hr + Decimal.Round(min / 60m, 2);
                    _context.Entry(dtl).State = EntityState.Modified;
                    _context.SaveChanges();
                }

                // Return ViewComponent for ajax request.
                return ViewComponent("RepEmpList", new { id = repairEmp.DocId });
            }
            else
            {
                string msg = "";
                foreach (var error in ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors))
                {
                    msg += error.ErrorMessage + Environment.NewLine;
                }
                throw new Exception(msg);
            }

        }

        public ActionResult GetEmpList(string docId)
        {
            return ViewComponent("RepEmpList", new { id = docId });
        }

        public ActionResult Delete(string id, string uName)
        {
            var uid = _context.AppUsers.Where(u => u.UserName == uName).FirstOrDefault().Id;
            try
            {
                RepairEmpModel repairEmp = _context.RepairEmps.Find(id, uid);
                _context.RepairEmps.Remove(repairEmp);
                _context.SaveChanges();
                
                // Recount the all repair time, and set value to RepairDtl.
                RepairDtlModel dtl = _context.RepairDtls.Where(d => d.DocId == repairEmp.DocId)
                                                        .FirstOrDefault();
                if (dtl != null)
                {
                    int hr = _context.RepairEmps.Where(p => p.DocId == repairEmp.DocId)
                                                .Select(p => p.Hour)
                                                .DefaultIfEmpty(0).Sum();
                    decimal min = _context.RepairEmps.Where(p => p.DocId == repairEmp.DocId)
                                                     .Select(p => p.Minute)
                                                     .DefaultIfEmpty(0).Sum();
                    dtl.Hour = hr + Decimal.Round(min / 60m, 2);
                    _context.Entry(dtl).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                return new JsonResult(repairEmp)
                {
                    Value = new { success = true, error = "" }
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}