using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Models;
using EDIS.Models.Identity;
using EDIS.Repositories;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class KeepEmpController : Controller
    {
        private readonly BMEDDbContext _context;

        public KeepEmpController(BMEDDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Edit(KeepEmpModel keepEmp)
        {
            if (ModelState.IsValid)
            {
                var ExistKeepEmp = _context.BMEDKeepEmps.Find(keepEmp.DocId, keepEmp.UserId);
                if (ExistKeepEmp != null)
                {
                    return new JsonResult(keepEmp)
                    {
                        Value = new { isExist = true, error = "資料已存在!" }
                    };
                }
                else
                {
                    _context.BMEDKeepEmps.Add(keepEmp);
                    _context.SaveChanges();
                }

                // Recount the all keep time, and set value to KeepDtl.
                KeepDtlModel dtl = _context.BMEDKeepDtls.Where(d => d.DocId == keepEmp.DocId)
                                                        .FirstOrDefault();
                if (dtl != null)
                {
                    int hr = _context.BMEDKeepEmps.Where(p => p.DocId == keepEmp.DocId)
                                                  .Select(p => p.Hour)
                                                  .DefaultIfEmpty(0).Sum();
                    decimal min = _context.BMEDKeepEmps.Where(p => p.DocId == keepEmp.DocId)
                                                       .Select(p => p.Minute)
                                                       .DefaultIfEmpty(0).Sum();
                    dtl.Hours = hr + Decimal.Round(min / 60m, 2);
                    _context.Entry(dtl).State = EntityState.Modified;
                    _context.SaveChanges();
                }

                // Return ViewComponent for ajax request.
                return ViewComponent("BMEDKeepEmpList", new { id = keepEmp.DocId, viewType = "Edit" });
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
            return ViewComponent("BMEDKeepEmpList", new { id = docId, viewType = "Edit" });
        }

        [HttpPost]
        public ActionResult Delete(string id, string uName)
        {
            var uid = _context.AppUsers.Where(u => u.UserName == uName).FirstOrDefault().Id;
            try
            {
                KeepEmpModel keepEmp = _context.BMEDKeepEmps.Find(id, uid);
                _context.BMEDKeepEmps.Remove(keepEmp);
                _context.SaveChanges();

                // Recount the all keep time, and set value to KeepDtl.
                KeepDtlModel dtl = _context.BMEDKeepDtls.Where(d => d.DocId == keepEmp.DocId)
                                                        .FirstOrDefault();
                if (dtl != null)
                {
                    int hr = _context.BMEDKeepEmps.Where(p => p.DocId == keepEmp.DocId)
                                                  .Select(p => p.Hour)
                                                  .DefaultIfEmpty(0).Sum();
                    decimal min = _context.BMEDKeepEmps.Where(p => p.DocId == keepEmp.DocId)
                                                       .Select(p => p.Minute)
                                                       .DefaultIfEmpty(0).Sum();
                    dtl.Hours = hr + Decimal.Round(min / 60m, 2);
                    _context.Entry(dtl).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                return new JsonResult(keepEmp)
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