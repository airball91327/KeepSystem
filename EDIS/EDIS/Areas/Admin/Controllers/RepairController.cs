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
using EDIS.Repositories;
using EDIS.Models.RepairModels;

namespace EDIS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class RepairController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;

        public RepairController(ApplicationDbContext context,
                                IRepository<AppUserModel, int> userRepo,
                                CustomUserManager customUserManager,
                                CustomRoleManager customRoleManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
            roleManager = customRoleManager;
        }

        // Get: Admin/Repair/
        public IActionResult Index(string docId = null)
        {
            ViewData["DOCID"] = docId;
            return View();
        }

        // Get: Admin/Repair/Edit/5
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            RepairModel repair = _context.Repairs.Find(id);
            if (repair == null)
            {
                return StatusCode(404);
            }
            /* Get and set value for NotMapped fields. */
            if (!string.IsNullOrEmpty(repair.Building))
            {
                int buildingId = Convert.ToInt32(repair.Building);
                repair.BuildingName = _context.Buildings.Find(buildingId).BuildingName;
                if (!string.IsNullOrEmpty(repair.Floor))
                {
                    repair.FloorName = _context.Floors.Find(buildingId, repair.Floor).FloorName;
                    if (!string.IsNullOrEmpty(repair.Area))
                    {
                        repair.AreaName = _context.Places.Find(buildingId, repair.Floor, repair.Area).PlaceName;
                    }
                }
            }
            repair.CheckerName = _context.AppUsers.Find(repair.CheckerId).FullName;
            return View(repair);
        }

        // POST: Admin/Repair/Edit/5
        [HttpPost]
        public IActionResult Edit(RepairModel repairModel)
        {
            RepairModel repair = _context.Repairs.Find(repairModel.DocId);
            var dpt = _context.Departments.Find(repairModel.AccDpt);
            if(dpt == null)
            {
                ModelState.AddModelError("AccDpt", "查無部門代號!");
                return View(repair);
            }
            if (repair != null)
            {
                repair.AccDpt = repairModel.AccDpt;
                repair.Ext = repairModel.Ext;
                repair.Mvpn = repairModel.Mvpn;
                repair.TroubleDes = repairModel.TroubleDes;
                repair.RepType = repairModel.RepType;

                _context.Entry(repair).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index", new { docId = repair.DocId });
            }
            return View(repair);
        }

        // POST: Admin/Repair/Details/5
        [HttpPost]
        public IActionResult Details(string qtyDocId)
        {
            string docId = qtyDocId.Trim();
            if (qtyDocId == null)
            {
                return BadRequest();
            }
            RepairModel repair = _context.Repairs.Find(docId);
            if (repair == null)
            {
                return StatusCode(404);
            }
            /* Get and set value for NotMapped fields. */
            if (!string.IsNullOrEmpty(repair.Building))
            {
                int buildingId = Convert.ToInt32(repair.Building);
                repair.BuildingName = _context.Buildings.Find(buildingId).BuildingName;
                if (!string.IsNullOrEmpty(repair.Floor))
                {
                    repair.FloorName = _context.Floors.Find(buildingId, repair.Floor).FloorName;
                    if (!string.IsNullOrEmpty(repair.Area))
                    {
                        repair.AreaName = _context.Places.Find(buildingId, repair.Floor, repair.Area).PlaceName;
                    }
                }
            }
            repair.CheckerName = _context.AppUsers.Find(repair.CheckerId).FullName;
            return View(repair);
        }

        // Get: Admin/Repair/EditRepFlow
        public IActionResult EditRepFlow()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EditRepFlow(string qtyDocId)
        {
            string docId = qtyDocId.Trim();
            var repairFlow = _context.RepairFlows.Where(rf => rf.DocId == docId).ToList();
            string repairStatus;

            if(repairFlow.Count() == 0)
            {
                string msg = "查無資料!!";
                return BadRequest(msg);
            }
            else
            {
                repairStatus = repairFlow.OrderBy(rf => rf.StepId).LastOrDefault().Status;

                if (repairStatus == "3")
                {
                    string msg = "此案件已廢除!!";
                    return BadRequest(msg);
                }
                else
                {
                    /* Insert values. */
                    AssignModel assign = new AssignModel();
                    assign.DocId = docId;
                    assign.AssignOpn = repairFlow.OrderBy(rf => rf.StepId).LastOrDefault().Opinions;

                    List<SelectListItem> listItem = new List<SelectListItem>();
                    listItem.Add(new SelectListItem { Text = "申請人", Value = "申請人" });
                    listItem.Add(new SelectListItem { Text = "驗收人", Value = "驗收人" });
                    listItem.Add(new SelectListItem { Text = "單位主管", Value = "單位主管" });
                    listItem.Add(new SelectListItem { Text = "單位主任", Value = "單位主任" });
                    listItem.Add(new SelectListItem { Text = "單位副院長", Value = "單位副院長" });
                    listItem.Add(new SelectListItem { Text = "工務/營建工程師", Value = "工務/營建工程師" });
                    listItem.Add(new SelectListItem { Text = "工務主管", Value = "工務主管" });
                    listItem.Add(new SelectListItem { Text = "工務主任", Value = "工務主任" });
                    listItem.Add(new SelectListItem { Text = "營建主管", Value = "營建主管" });
                    listItem.Add(new SelectListItem { Text = "營建主任", Value = "營建主任" });
                    listItem.Add(new SelectListItem { Text = "工務經辦", Value = "工務經辦" });
                    listItem.Add(new SelectListItem { Text = "列管財產負責人", Value = "列管財產負責人" });
                    listItem.Add(new SelectListItem { Text = "固資財產負責人", Value = "固資財產負責人" });
                    listItem.Add(new SelectListItem { Text = "其他", Value = "其他" });
                    ViewData["FlowCls"] = new SelectList(listItem, "Value", "Text", "");

                    List<SelectListItem> listItem3 = new List<SelectListItem>();
                    listItem3.Add(new SelectListItem { Text = "", Value = "" });
                    ViewData["FlowUid"] = new SelectList(listItem3, "Value", "Text", "");

                    return View("EditFlowList", assign);
                }
            }
        }

        [HttpPost]
        public ActionResult EditNextFlow(AssignModel assign)
        {
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            var repairFlow = _context.RepairFlows.Where(f => f.DocId == assign.DocId).ToList();
            string lastStatus = repairFlow.OrderBy(f => f.StepId).LastOrDefault().Status;
            RepairFlowModel rf;

            if (ModelState.IsValid)
            {
                if (lastStatus == "2")
                {
                    rf = _context.RepairFlows.Where(f => f.DocId == assign.DocId && f.Status == "2").FirstOrDefault();
                }
                else
                {
                    rf = _context.RepairFlows.Where(f => f.DocId == assign.DocId && f.Status == "?").FirstOrDefault();
                }

                //轉單
                assign.AssignOpn += "【已經由轉單人員[" + ur.FullName + "]轉單】";
                rf.Opinions = assign.AssignOpn;
                rf.Status = "1";
                rf.Rtt = DateTime.Now;
                rf.Rtp = ur.Id;
                _context.Entry(rf).State = EntityState.Modified;
                _context.SaveChanges();
                //
                RepairFlowModel flow = new RepairFlowModel();
                flow.DocId = assign.DocId;
                flow.StepId = rf.StepId + 1;
                flow.UserId = assign.FlowUid.Value;
                flow.UserName = _context.AppUsers.Find(assign.FlowUid.Value).FullName;
                flow.Status = "?";
                flow.Cls = assign.FlowCls;
                flow.Rtt = DateTime.Now;
                _context.RepairFlows.Add(flow);
                _context.SaveChanges();

                return new JsonResult(assign)
                {
                    Value = new { success = true, error = "" }
                };
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

    }
}