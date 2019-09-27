using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Areas.BMED.Repositories;
using EDIS.Repositories;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class RepairEmpController : Controller
    {
        private readonly BMEDDbContext _context;
        private readonly BMEDIRepository<RepairModel, string> _repRepo;
        private readonly BMEDIRepository<RepairDtlModel, string> _repdtlRepo;
        private readonly BMEDIRepository<RepairFlowModel, string[]> _repflowRepo;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly IRepository<DepartmentModel, string> _dptRepo;
        private readonly BMEDIRepository<RepairEmpModel, string[]> _repempRepo;
        private readonly IEmailSender _emailSender;
        private readonly CustomUserManager userManager;

        public RepairEmpController(BMEDDbContext context,
                                   BMEDIRepository<RepairModel, string> repairRepo,
                                   BMEDIRepository<RepairDtlModel, string> repairdtlRepo,
                                   BMEDIRepository<RepairFlowModel, string[]> repairflowRepo,
                                   IRepository<AppUserModel, int> userRepo,
                                   IRepository<DepartmentModel, string> dptRepo,
                                   BMEDIRepository<RepairEmpModel, string[]> repairempRepo,
                                   IEmailSender emailSender,
                                   CustomUserManager customUserManager)
        {
            _context = context;
            _repRepo = repairRepo;
            _repdtlRepo = repairdtlRepo;
            _repflowRepo = repairflowRepo;
            _userRepo = userRepo;
            _dptRepo = dptRepo;
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
                _context.BMEDRepairEmps.Add(repairEmp);
                _context.SaveChanges();
                // Recount the all repair time, and set value to RepairDtl.
                RepairDtlModel dtl = _context.BMEDRepairDtls.Where(d => d.DocId == repairEmp.DocId)
                                                            .FirstOrDefault();
                if (dtl != null)
                {
                    int hr = _context.BMEDRepairEmps.Where(p => p.DocId == repairEmp.DocId)
                                                    .Select(p => p.Hour)
                                                    .DefaultIfEmpty(0).Sum();
                    decimal min = _context.BMEDRepairEmps.Where(p => p.DocId == repairEmp.DocId)
                                                         .Select(p => p.Minute)
                                                         .DefaultIfEmpty(0).Sum();
                    dtl.Hour = hr + Decimal.Round(min / 60m, 2);
                    _context.BMEDRepairDtls.Update(dtl);
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
                var ExistRepairEmp = _context.BMEDRepairEmps.Find(repairEmp.DocId, repairEmp.UserId);
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
                RepairDtlModel dtl = _context.BMEDRepairDtls.Where(d => d.DocId == repairEmp.DocId)
                                                            .FirstOrDefault();
                if (dtl != null)
                {
                    int hr = _context.BMEDRepairEmps.Where(p => p.DocId == repairEmp.DocId)
                                                    .Select(p => p.Hour)
                                                    .DefaultIfEmpty(0).Sum();
                    decimal min = _context.BMEDRepairEmps.Where(p => p.DocId == repairEmp.DocId)
                                                         .Select(p => p.Minute)
                                                         .DefaultIfEmpty(0).Sum();
                    dtl.Hour = hr + Decimal.Round(min / 60m, 2);
                    _context.Entry(dtl).State = EntityState.Modified;
                    _context.SaveChanges();
                }

                // Return ViewComponent for ajax request.
                return ViewComponent("BMEDRepEmpList", new { id = repairEmp.DocId, viewType = "Edit" });
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
            return ViewComponent("BMEDRepEmpList", new { id = docId, viewType = "Edit" });
        }

        [HttpPost]
        public ActionResult Delete(string id, string uName)
        {
            var uid = _context.AppUsers.Where(u => u.UserName == uName).FirstOrDefault().Id;
            try
            {
                RepairEmpModel repairEmp = _context.BMEDRepairEmps.Find(id, uid);
                _context.BMEDRepairEmps.Remove(repairEmp);
                _context.SaveChanges();
                
                // Recount the all repair time, and set value to RepairDtl.
                RepairDtlModel dtl = _context.BMEDRepairDtls.Where(d => d.DocId == repairEmp.DocId)
                                                            .FirstOrDefault();
                if (dtl != null)
                {
                    int hr = _context.BMEDRepairEmps.Where(p => p.DocId == repairEmp.DocId)
                                                    .Select(p => p.Hour)
                                                    .DefaultIfEmpty(0).Sum();
                    decimal min = _context.BMEDRepairEmps.Where(p => p.DocId == repairEmp.DocId)
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