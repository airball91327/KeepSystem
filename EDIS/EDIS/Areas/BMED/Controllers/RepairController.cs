using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Areas.BMED.Repositories;
using EDIS.Repositories;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;
using EDIS.Models;
using Zen.Barcode;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class RepairController : Controller
    {
        private readonly BMEDDbContext _context;
        private readonly BMEDIRepository<RepairModel, string> _repRepo;
        private readonly BMEDIRepository<RepairDtlModel, string> _repdtlRepo;
        private readonly BMEDIRepository<RepairFlowModel, string[]> _repflowRepo;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly IRepository<DepartmentModel, string> _dptRepo;
        private readonly IRepository<DocIdStore, string[]> _dsRepo;
        private readonly IEmailSender _emailSender;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;

        public RepairController(BMEDDbContext context,
                                BMEDIRepository<RepairModel, string> repairRepo,
                                BMEDIRepository<RepairDtlModel, string> repairdtlRepo,
                                BMEDIRepository<RepairFlowModel, string[]> repairflowRepo,
                                IRepository<AppUserModel, int> userRepo,
                                IRepository<DepartmentModel, string> dptRepo,
                                IRepository<DocIdStore, string[]> dsRepo,
                                IEmailSender emailSender,
                                CustomUserManager customUserManager,
                                CustomRoleManager customRoleManager)
        {
            _context = context;
            _repRepo = repairRepo;
            _repdtlRepo = repairdtlRepo;
            _repflowRepo = repairflowRepo;
            _userRepo = userRepo;
            _dptRepo = dptRepo;
            _dsRepo = dsRepo;
            _emailSender = emailSender;
            userManager = customUserManager;
            roleManager = customRoleManager;
        }

        // GET: /<controller>/
        //Not Used
        public IActionResult Index()
        {
            List<SelectListItem> FlowlistItem = new List<SelectListItem>();
            FlowlistItem.Add(new SelectListItem { Text = "待處理", Value = "待處理" });
            FlowlistItem.Add(new SelectListItem { Text = "已處理", Value = "已處理" });
            FlowlistItem.Add(new SelectListItem { Text = "已結案", Value = "已結案" });
            ViewData["FLOWTYPE"] = new SelectList(FlowlistItem, "Value", "Text");
            //
            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem {
                Text = "醫工部",
                Value = "8420"
            });
            ViewData["ACCDPT"] = new SelectList(listItem, "Value", "Text");
            ViewData["APPLYDPT"] = new SelectList(listItem, "Value", "Text");
            return View();
        }

        // POST: BMED/Keep/Index
        [HttpPost]
        public IActionResult Index(QryRepListData qdata)
        {
            string docid = qdata.BMEDqtyDOCID;
            string ano = qdata.BMEDqtyASSETNO;
            string acc = qdata.BMEDqtyACCDPT;
            string aname = qdata.BMEDqtyASSETNAME;
            string ftype = qdata.BMEDqtyFLOWTYPE;
            string dptid = qdata.BMEDqtyDPTID;
            string qtyDate1 = qdata.BMEDqtyApplyDateFrom;
            string qtyDate2 = qdata.BMEDqtyApplyDateTo;
            string qtyDealStatus = qdata.BMEDqtyDealStatus;
            string qtyIsCharged = qdata.BMEDqtyIsCharged;
            string qtyDateType = qdata.BMEDqtyDateType;
            bool searchAllDoc = qdata.BMEDqtySearchAllDoc;
            string qtyEngCode = qdata.BMEDqtyEngCode;
            string qtyTicketNo = qdata.BMEDqtyTicketNo;
            string qtyVendor = qdata.BMEDqtyVendor;

            if (qtyEngCode != null)
            {
                searchAllDoc = true;
            }

            DateTime applyDateFrom = DateTime.Now;
            DateTime applyDateTo = DateTime.Now;
            /* Dealing search by date. */
            if (qtyDate1 != null && qtyDate2 != null)// If 2 date inputs have been insert, compare 2 dates.
            {
                DateTime date1 = DateTime.Parse(qtyDate1);
                DateTime date2 = DateTime.Parse(qtyDate2);
                int result = DateTime.Compare(date1, date2);
                if (result < 0)
                {
                    applyDateFrom = date1.Date;
                    applyDateTo = date2.Date;
                }
                else if (result == 0)
                {
                    applyDateFrom = date1.Date;
                    applyDateTo = date1.Date;
                }
                else
                {
                    applyDateFrom = date2.Date;
                    applyDateTo = date1.Date;
                }
            }
            else if (qtyDate1 == null && qtyDate2 != null)
            {
                applyDateFrom = DateTime.Parse(qtyDate2);
                applyDateTo = DateTime.Parse(qtyDate2);
            }
            else if (qtyDate1 != null && qtyDate2 == null)
            {
                applyDateFrom = DateTime.Parse(qtyDate1);
                applyDateTo = DateTime.Parse(qtyDate1);
            }


            List<RepairListVModel> rv = new List<RepairListVModel>();
            /* Get login user. */
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();

            var rps = _context.BMEDRepairs.ToList();
            if (!string.IsNullOrEmpty(docid))   //表單編號
            {
                docid = docid.Trim();
                rps = rps.Where(v => v.DocId == docid).ToList();
            }
            if (!string.IsNullOrEmpty(ano))     //財產編號
            {
                rps = rps.Where(v => v.AssetNo == ano).ToList();
            }
            if (!string.IsNullOrEmpty(dptid))   //所屬部門編號
            {
                rps = rps.Where(v => v.DptId == dptid).ToList();
            }
            if (!string.IsNullOrEmpty(acc))     //成本中心
            {
                rps = rps.Where(v => v.AccDpt == acc).ToList();
            }
            if (!string.IsNullOrEmpty(aname))   //財產名稱
            {
                rps = rps.Where(v => v.AssetName != null)
                         .Where(v => v.AssetName.Contains(aname))
                         .ToList();
            }
            if (!string.IsNullOrEmpty(qtyTicketNo))     //發票號碼
            {
                qtyTicketNo = qtyTicketNo.ToUpper();
                var resultDocIds = _context.BMEDRepairCosts.Include(rc => rc.TicketDtl)
                                                           .Where(rc => rc.TicketDtl.TicketDtlNo == qtyTicketNo)
                                                           .Select(rc => rc.DocId).Distinct();
                rps = (from r in rps
                       where resultDocIds.Any(val => r.DocId.Contains(val))
                       select r).ToList();
            }
            if (!string.IsNullOrEmpty(qtyVendor))   //廠商關鍵字
            {
                var resultDocIds = _context.BMEDRepairCosts.Include(rc => rc.TicketDtl)
                                                           .Where(rc => rc.VendorName.Contains(qtyVendor))
                                                           .Select(rc => rc.DocId).Distinct();
                rps = (from r in rps
                       where resultDocIds.Any(val => r.DocId.Contains(val))
                       select r).ToList();
            }
            /* Search date by DateType.(ApplyDate) */
            if (string.IsNullOrEmpty(qtyDate1) == false || string.IsNullOrEmpty(qtyDate2) == false)
            {
                if (qtyDateType == "申請日")
                {
                    rps = rps.Where(v => v.ApplyDate >= applyDateFrom && v.ApplyDate <= applyDateTo).ToList();
                }
            }

            /* If no search result. */
            if (rps.Count() == 0)
            {
                return View("List", rv);
            }

            switch (ftype)
            {
                /* 與登入者相關且流程不在該登入者身上的文件 */
                case "流程中":
                    rps.Join(_context.BMEDRepairFlows.Where(f2 => f2.UserId == ur.Id && f2.Status == "1")
                       .Select(f => f.DocId).Distinct(),
                               r => r.DocId, f2 => f2, (r, f2) => r)
                       .Join(_context.BMEDRepairFlows.Where(f => f.Status == "?" && f.UserId != ur.Id),
                       r => r.DocId, f => f.DocId,
                       (r, f) => new
                       {
                           repair = r,
                           flow = f
                       })
                       .Join(_context.BMEDRepairDtls, m => m.repair.DocId, d => d.DocId,
                       (m, d) => new
                       {
                           repair = m.repair,
                           flow = m.flow,
                           repdtl = d
                       })
                       .Join(_context.Departments, j => j.repair.AccDpt, d => d.DptId,
                       (j, d) => new
                       {
                           repair = j.repair,
                           flow = j.flow,
                           repdtl = j.repdtl,
                           dpt = d
                       })
                       .ToList()
                       .ForEach(j => rv.Add(new RepairListVModel
                       {
                           DocType = "醫工請修",
                           RepType = j.repair.RepType,
                           DocId = j.repair.DocId,
                           ApplyDate = j.repair.ApplyDate,
                           PlaceLoc = j.repair.PlaceLoc,
                           ApplyDpt = j.repair.DptId,
                           AccDpt = j.repair.AccDpt,
                           AccDptName = j.dpt.Name_C,
                           TroubleDes = j.repair.TroubleDes,
                           DealState = _context.BMEDDealStatuses.Find(j.repdtl.DealState).Title,
                           DealDes = j.repdtl.DealDes,
                           Cost = j.repdtl.Cost,
                           Days = DateTime.Now.Subtract(j.repair.ApplyDate).Days,
                           Flg = j.flow.Status,
                           FlowUid = j.flow.UserId,
                           FlowCls = j.flow.Cls,
                           FlowDptId = _context.AppUsers.Find(j.flow.UserId).DptId,
                           EndDate = j.repdtl.EndDate,
                           IsCharged = j.repdtl.IsCharged,
                           repdata = j.repair,
                           ExFlowUid = _context.BMEDRepairFlows.Where(r => r.DocId == j.flow.DocId).OrderByDescending(r => r.StepId).Skip(1).FirstOrDefault().UserId,
                           ExFlowCls = _context.BMEDRepairFlows.Where(r => r.DocId == j.flow.DocId).OrderByDescending(r => r.StepId).Skip(1).FirstOrDefault().Cls
                       }));
                    break;
                /* 與登入者相關且結案的文件 */
                case "已結案":
                    /* Get all closed repair docs. */
                    List<RepairFlowModel> rf = _context.BMEDRepairFlows.Where(f => f.Status == "2").ToList();

                    if (userManager.IsInRole(User, "Admin") || userManager.IsInRole(User, "MedAdmin") || 
                        userManager.IsInRole(User, "Manager") || userManager.IsInRole(User, "MedEngineer"))
                    {
                        if (userManager.IsInRole(User, "Manager"))
                        {
                            rf = rf.Join(_context.BMEDRepairs.Where(r => r.AccDpt == ur.DptId),
                            f => f.DocId, r => r.DocId, (f, r) => f).ToList();
                        }
                        /* If no other search values, search the docs belong the login engineer. */
                        if (userManager.IsInRole(User, "MedEngineer") && searchAllDoc == false)
                        {
                            rf = rf.Join(_context.BMEDRepairFlows.Where(f2 => f2.UserId == ur.Id),
                                 f => f.DocId, f2 => f2.DocId, (f, f2) => f).ToList();
                        }
                    }
                    else /* If normal user, search the docs belong himself. */
                    {
                        rf = rf.Join(_context.BMEDRepairFlows.Where(f2 => f2.UserId == ur.Id),
                             f => f.DocId, f2 => f2.DocId, (f, f2) => f).ToList();
                    }

                    rf.Select(f => new
                    {
                        f.DocId,
                        f.UserId,
                        f.Status,
                        f.Cls
                    }).Distinct().Join(rps.DefaultIfEmpty(), f => f.DocId, r => r.DocId,
                    (f, r) => new
                    {
                        repair = r,
                        flow = f
                    })
                    .Join(_context.BMEDRepairDtls, m => m.repair.DocId, d => d.DocId,
                    (m, d) => new
                    {
                        repair = m.repair,
                        flow = m.flow,
                        repdtl = d
                    })
                    .Join(_context.Departments, j => j.repair.AccDpt, d => d.DptId,
                    (j, d) => new
                    {
                        repair = j.repair,
                        flow = j.flow,
                        repdtl = j.repdtl,
                        dpt = d
                    }).ToList()
                    .ForEach(j => rv.Add(new RepairListVModel
                    {
                        DocType = "醫工請修",
                        RepType = j.repair.RepType,
                        DocId = j.repair.DocId,
                        ApplyDate = j.repair.ApplyDate,
                        PlaceLoc = j.repair.PlaceLoc,
                        ApplyDpt = j.repair.DptId,
                        AccDpt = j.repair.AccDpt,
                        AccDptName = j.dpt.Name_C,
                        TroubleDes = j.repair.TroubleDes,
                        DealState = _context.BMEDDealStatuses.Find(j.repdtl.DealState).Title,
                        DealDes = j.repdtl.DealDes,
                        Cost = j.repdtl.Cost,
                        Days = DateTime.Now.Subtract(j.repair.ApplyDate).Days,
                        Flg = j.flow.Status,
                        FlowUid = j.flow.UserId,
                        FlowCls = j.flow.Cls,
                        FlowDptId = _context.AppUsers.Find(j.flow.UserId).DptId,
                        EndDate = j.repdtl.EndDate,
                        CloseDate = j.repdtl.CloseDate.Value.Date,
                        IsCharged = j.repdtl.IsCharged,
                        repdata = j.repair
                    }));
                    break;
                /* 與登入者相關且流程在該登入者身上的文件 */
                case "待簽核":
                    /* Get all dealing repair docs. */
                    var repairFlows = _context.BMEDRepairFlows.Join(rps.DefaultIfEmpty(), f => f.DocId, r => r.DocId,
                    (f, r) => new
                    {
                        repair = r,
                        flow = f
                    }).ToList();

                    if (userManager.IsInRole(User, "Admin") || userManager.IsInRole(User, "MedAdmin") || 
                        userManager.IsInRole(User, "MedEngineer"))
                    {
                        /* If has other search values, search all RepairDocs which flowCls is in engineer. */
                        /* Else return the docs belong the login engineer.  */
                        if (userManager.IsInRole(User, "MedEngineer") && searchAllDoc == true)
                        {
                            repairFlows = repairFlows.Where(f => f.flow.Status == "?" && f.flow.Cls.Contains("工程師")).ToList();
                            if (!string.IsNullOrEmpty(qtyEngCode))  //工程師搜尋
                            {
                                repairFlows = repairFlows.Where(f => f.repair.EngId == Convert.ToInt32(qtyEngCode)).ToList();
                            }
                        }
                        else
                        {
                            /* 個人或同部門結案案件 */
                            //repairFlows = repairFlows.Where(f => (f.flow.Status == "?" && f.flow.UserId == ur.Id) ||
                            //                                     (f.flow.Status == "?" && f.flow.Cls == "驗收人" &&
                            //                                      _context.AppUsers.Find(f.flow.UserId).DptId == ur.DptId)).ToList();

                            /* 個人案件 */
                            repairFlows = repairFlows.Where(f => (f.flow.Status == "?" && f.flow.UserId == ur.Id)).ToList();
                        }
                    }
                    else
                    {
                        /* 個人或同部門結案案件 */
                        //repairFlows = repairFlows.Where(f => (f.flow.Status == "?" && f.flow.UserId == ur.Id) ||
                        //                                     (f.flow.Status == "?" && f.flow.Cls == "驗收人" &&
                        //                                       _context.AppUsers.Find(f.flow.UserId).DptId == ur.DptId)).ToList();

                        /* 個人案件 */
                        repairFlows = repairFlows.Where(f => (f.flow.Status == "?" && f.flow.UserId == ur.Id)).ToList();
                    }

                    repairFlows.Join(_context.BMEDRepairDtls, m => m.repair.DocId, d => d.DocId,
                    (m, d) => new
                    {
                        repair = m.repair,
                        flow = m.flow,
                        repdtl = d
                    })
                    .Join(_context.Departments, j => j.repair.AccDpt, d => d.DptId,
                    (j, d) => new
                    {
                        repair = j.repair,
                        flow = j.flow,
                        repdtl = j.repdtl,
                        dpt = d
                    }).ToList()
                    .ForEach(j => rv.Add(new RepairListVModel
                    {
                        DocType = "醫工請修",
                        RepType = j.repair.RepType,
                        DocId = j.repair.DocId,
                        ApplyDate = j.repair.ApplyDate,
                        PlaceLoc = j.repair.PlaceLoc,
                        ApplyDpt = j.repair.DptId,
                        AccDpt = j.repair.AccDpt,
                        AccDptName = j.dpt.Name_C,
                        TroubleDes = j.repair.TroubleDes,
                        DealState = _context.BMEDDealStatuses.Find(j.repdtl.DealState).Title,
                        DealDes = j.repdtl.DealDes,
                        Cost = j.repdtl.Cost,
                        Days = DateTime.Now.Subtract(j.repair.ApplyDate).Days,
                        Flg = j.flow.Status,
                        FlowUid = j.flow.UserId,
                        FlowCls = j.flow.Cls,
                        FlowDptId = _context.AppUsers.Find(j.flow.UserId).DptId,
                        EndDate = j.repdtl.EndDate,
                        IsCharged = j.repdtl.IsCharged,
                        repdata = j.repair
                    }));
                    break;
            };

            /* 設備編號"有"、"無"的對應，"有"讀取table相關data，"無"只顯示申請人輸入的設備名稱 */
            foreach (var item in rv)
            {
                if (!string.IsNullOrEmpty(item.repdata.AssetNo))
                {
                    var asset = _context.BMEDAssets.Where(a => a.AssetNo == item.repdata.AssetNo).FirstOrDefault();
                    if (asset != null)
                    {
                        item.AssetNo = asset.AssetNo;
                        item.AssetName = asset.Cname;
                        item.Brand = asset.Brand;
                        item.Type = asset.Type;
                    }
                }
                else
                {
                    item.AssetName = item.repdata.AssetName;
                }
            }

            /* Search date by DateType. */
            if (string.IsNullOrEmpty(qtyDate1) == false || string.IsNullOrEmpty(qtyDate2) == false)
            {
                if (qtyDateType == "結案日")
                {
                    rv = rv.Where(v => v.CloseDate >= applyDateFrom && v.CloseDate <= applyDateTo).ToList();
                }
                else if (qtyDateType == "完工日")
                {
                    rv = rv.Where(v => v.EndDate >= applyDateFrom && v.EndDate <= applyDateTo).ToList();
                }
            }

            /* Sorting search result. */
            if (rv.Count() != 0)
            {
                if (qtyDateType == "結案日")
                {
                    rv = rv.OrderByDescending(r => r.CloseDate).ThenByDescending(r => r.DocId).ToList();
                }
                else if (qtyDateType == "完工日")
                {
                    rv = rv.OrderByDescending(r => r.EndDate).ThenByDescending(r => r.DocId).ToList();
                }
                else
                {
                    rv = rv.OrderByDescending(r => r.ApplyDate).ThenByDescending(r => r.DocId).ToList();
                }
            }

            /* Search dealStatus. */
            if (!string.IsNullOrEmpty(qtyDealStatus))
            {
                rv = rv.Where(r => r.DealState == qtyDealStatus).ToList();
            }
            /* Search IsCharged. */
            if (!string.IsNullOrEmpty(qtyIsCharged))
            {
                rv = rv.Where(r => r.IsCharged == qtyIsCharged).ToList();
            }

            return View("List", rv);
        }
        [Authorize]
        public IActionResult Create()
        {
            var test = _context.BMEDRepairs.ToList();
            RepairModel repair = new RepairModel();
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            var dpt = _dptRepo.FindById(ur.DptId);
            repair.DocId = GetID2();
            repair.UserId = ur.Id;
            repair.UserName = ur.FullName;
            repair.UserAccount = ur.UserName;
            repair.DptId = ur.DptId;
            repair.DptName = dpt.Name_C;
            repair.AccDpt = ur.DptId;
            repair.AccDptName = dpt.Name_C;
            repair.ApplyDate = DateTime.Now;

            /* 擷取該使用者單位底下所有人員 */
            var dptUsers = _context.AppUsers.Where(a => a.DptId == ur.DptId).ToList();
            List<SelectListItem> dptMemberList = new List<SelectListItem>();
            foreach (var item in dptUsers)
            {
                dptMemberList.Add(new SelectListItem
                {
                    Text = item.FullName,
                    Value = item.Id.ToString()
                });
            }
            ViewData["DptMembers"] = new SelectList(dptMemberList, "Value", "Text");

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
            ViewData["AllEngs"] = new SelectList(list, "Value", "Text");
            repair.CheckerId = ur.Id;

            return View(repair);
        }

        [HttpPost]
        public IActionResult Create([FromForm]RepairModel repair)
        {
            /* 如有指定工程師，將預設工程師改為指定 */
            if(repair.PrimaryEngId != null && repair.PrimaryEngId != 0)
            {
                repair.EngId = Convert.ToInt32(repair.PrimaryEngId);
            }
            /* 如有代理人，將工程師改為代理人*/
            //var subStaff = _context.EngSubStaff.SingleOrDefault(e => e.EngId == repair.EngId);
            //if(subStaff != null)
            //{
            //    int startDate = Convert.ToInt32(subStaff.StartDate.ToString("yyyyMMdd"));
            //    int endDate = Convert.ToInt32(subStaff.EndDate.ToString("yyyyMMdd"));
            //    int today = Convert.ToInt32(DateTime.UtcNow.AddHours(08).ToString("yyyyMMdd"));
            //    /* 如在代理期間內，將代理人指定為負責工程師 */
            //    if(today >= startDate && today <= endDate)
            //    {
            //        repair.EngId = subStaff.SubstituteId;
            //    }
            //}

            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).First();
            string msg = "";
            try
            {
                if (ModelState.IsValid)
                {
                    // Create Repair Doc.
                    //repair.DocId = "12345678";
                    repair.ApplyDate = DateTime.Now;
                    _repRepo.Create(repair);
                    //_repRepo.Update(repair);

                    // Create Repair Details.
                    RepairDtlModel dtl = new RepairDtlModel();
                    var notInExceptDevice = _context.ExceptDevice.Find(repair.AssetNo);
                    /* If can find data in ExceptDevice table, the device is "not" 統包. 
                     * It means if value is "Y", the device is 統包
                     */
                    if (notInExceptDevice == null)
                    {
                        dtl.NotInExceptDevice = "Y";
                    }
                    else
                    {
                        dtl.NotInExceptDevice = "N";
                    }
                    dtl.DocId = repair.DocId;
                    dtl.DealState = 1;  // 處理狀態"未處理"
                    _repdtlRepo.Create(dtl);

                    //Create first Repair Flow.
                    RepairFlowModel flow = new RepairFlowModel();
                    flow.DocId = repair.DocId;
                    flow.StepId = 1;
                    flow.UserId = ur.Id;
                    //flow.UserId = userManager.GetCurrentUserId(User);
                    flow.Status = "1";  // 流程狀態"已處理"
                    flow.Rtp = ur.Id;
                    //flow.Rtp = userManager.GetCurrentUserId(User);
                    flow.Rtt = DateTime.Now;
                    flow.Cls = "申請人";
                    _repflowRepo.Create(flow);

                    // Create next flow.
                    flow = new RepairFlowModel();
                    flow.DocId = repair.DocId;
                    flow.StepId = 2;
                    flow.UserId = repair.EngId;
                    flow.Status = "?";  // 狀態"未處理"
                    flow.Rtt = DateTime.Now;
                    flow.Cls = "設備工程師";

                    _repflowRepo.Create(flow);

                    //Send Mail 
                    //To user and the next flow user.
                    Tmail mail = new Tmail();
                    string body = "";
                    var mailToUser = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
                    mail.from = new System.Net.Mail.MailAddress(mailToUser.Email); //u.Email
                    mailToUser = _context.AppUsers.Find(flow.UserId);
                    mail.to = new System.Net.Mail.MailAddress(mailToUser.Email); //u.Email
                    //mail.cc = new System.Net.Mail.MailAddress("344027@cch.org.tw");
                    mail.message.Subject = "醫工工務智能保修系統[醫工請修案]：設備名稱： " + repair.AssetName;
                    body += "<p>表單編號：" + repair.DocId + "</p>";
                    body += "<p>申請日期：" + repair.ApplyDate.ToString("yyyy/MM/dd") + "</p>";
                    body += "<p>申請人：" + repair.UserName + "</p>";
                    body += "<p>財產編號：" + repair.AssetNo + "</p>";
                    body += "<p>設備名稱：" + repair.AssetName + "</p>";
                    body += "<p>故障描述：" + repair.TroubleDes + "</p>";
                    //body += "<p>請修地點：" + repair.PlaceLoc + " " + repair.BuildingName + " " + repair.FloorName + " " + repair.AreaName + "</p>";
                    body += "<p>放置地點：" + repair.PlaceLoc + "</p>";
                    body += "<p><a href='http://dms.cch.org.tw/EDIS/Account/Login'" + "?docId=" + repair.DocId + "&dealType=BMEDRepEdit" + ">處理案件</a></p>";
                    body += "<br/>";
                    body += "<h3>此封信件為系統通知郵件，請勿回覆。</h3>";
                    body += "<br/>";
                    //body += "<h3 style='color:red'>如有任何疑問請聯絡工務部，分機3033或7033。<h3>";
                    mail.message.Body = body;
                    mail.message.IsBodyHtml = true;
                    mail.SendMail();

                    return Ok(repair);
                }
                else
                {
                    foreach (var error in ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors))
                    {
                        msg += error.ErrorMessage + Environment.NewLine;
                    }
                }
            }catch(Exception ex)
            {
                msg = ex.Message;
            }

            return BadRequest(msg);
        }

        public IActionResult Edit(string id, int page)
        {
            ViewData["Page"] = page;
            RepairModel repair = _context.BMEDRepairs.Find(id);
            if (repair == null)
            {
                return StatusCode(404);
            }
            return View(repair);
        }

        // POST: BMED/Repair/Update/5
        [HttpPost]
        public IActionResult Update(RepairModel repairModel)
        {
            RepairModel repair = _context.BMEDRepairs.Find(repairModel.DocId);
            if (repair == null)
            {
                return BadRequest("查無案件!");
            }

            if (string.IsNullOrEmpty(repairModel.AccDpt))
            {
                return BadRequest("成本中心不可空白!");
            }

            repairModel.AccDpt = repairModel.AccDpt.Trim();
            var dpt = _context.Departments.Find(repairModel.AccDpt);
            if (dpt == null)
            {
                return BadRequest("此編號查無部門!");
            }
            repair.AccDpt = repairModel.AccDpt;
            _context.Entry(repair).State = EntityState.Modified;
            _context.SaveChanges();
            return PartialView("Update", repair);
        }

        public IActionResult Detail()
        {
            RepairModel repair = new RepairModel();

            return PartialView(repair);
        }

        public IActionResult List()
        {
            return PartialView();
        }
        public string GetID()
        {
            DocIdStore ds = new DocIdStore();
            ds.DocType = "醫工請修";
            string s = _dsRepo.Find(x => x.DocType == "醫工請修").Select(x => x.DocId).Max();
            string did = "";
            int yymm = (System.DateTime.Now.Year - 1911) * 100 + System.DateTime.Now.Month;
            if (!string.IsNullOrEmpty(s))
            {
                did = s;
            }
            if (did != "")
            {
                if (Convert.ToInt64(did) / 100000 == yymm)
                    did = Convert.ToString(Convert.ToInt64(did) + 1);
                else
                    did = Convert.ToString(yymm * 100000 + 1);
                ds.DocId = did;
                _dsRepo.Update(ds);
            }
            else
            {
                did = Convert.ToString(yymm * 100000 + 1);
                ds.DocId = did;
                _dsRepo.Create(ds);
            }

            return did;
        }

        public string GetID2()
        {
            string did = "";
            try
            {
                DocIdStore ds = new DocIdStore();
                ds.DocType = "醫工請修";
                string s = _dsRepo.Find(x => x.DocType == "醫工請修").Select(x => x.DocId).Max();
                int yymmdd = (System.DateTime.Now.Year - 1911) * 10000 + (System.DateTime.Now.Month) * 100 + System.DateTime.Now.Day;
                if (!string.IsNullOrEmpty(s))
                {
                    did = s;
                }
                if (did != "")
                {
                    if (Convert.ToInt64(did) / 1000 == yymmdd)
                        did = Convert.ToString(Convert.ToInt64(did) + 1);
                    else
                        did = Convert.ToString(yymmdd * 1000 + 1);
                    ds.DocId = did;
                    _dsRepo.Update(ds);
                }
                else
                {
                    did = Convert.ToString(yymmdd * 1000 + 1);
                    ds.DocId = did;
                    // 二次確認資料庫內沒該資料才新增。
                    var dataIsExist = _dsRepo.Find(x => x.DocType == "醫工請修");
                    if (dataIsExist.Count() == 0)
                    {
                        _dsRepo.Create(ds);
                    }
                }
            }
            catch (Exception e)
            {
                RedirectToAction("Create", "Repair", new { Area = "BMED" });
            }
            return did;
        }

        public IActionResult Views(string id)
        {
            RepairModel repair = _context.BMEDRepairs.Find(id);
            if (repair == null)
            {
                return StatusCode(404);
            }
            return View(repair);
        }

        [HttpPost]
        public IActionResult ResignEng(List<RepairListVModel> data, string BMEDassignEngId)
        {
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            int assignEngId = Convert.ToInt32(BMEDassignEngId);

            foreach(var item in data)
            {
                if (item.IsSelected)
                {
                    RepairModel repair = _context.BMEDRepairs.Find(item.DocId);
                    //指派工程師
                    repair.EngId = assignEngId;
                    _context.Entry(repair).State = EntityState.Modified;
                    _context.SaveChanges();

                    RepairFlowModel rf = _context.BMEDRepairFlows.Where(f => f.DocId == item.DocId && f.Status == "?").FirstOrDefault();
                    //轉送下一關卡
                    rf.Opinions = "[指派工程師]";
                    rf.Status = "1";
                    rf.Rtt = DateTime.Now;
                    rf.Rtp = ur.Id;
                    _context.Entry(rf).State = EntityState.Modified;
                    _context.SaveChanges();
                    //
                    RepairFlowModel flow = new RepairFlowModel();
                    flow.DocId = item.DocId;
                    flow.StepId = rf.StepId + 1;
                    flow.UserId = assignEngId;
                    flow.UserName = _context.AppUsers.Find(assignEngId).FullName;
                    flow.Status = "?";
                    flow.Cls = "設備工程師";
                    flow.Rtt = DateTime.Now;
                    _context.BMEDRepairFlows.Add(flow);
                    _context.SaveChanges();
                }
            }
             return View();
        }

        [HttpPost]
        public JsonResult GetDptName(string dptId)
        {
            var dpt = _context.Departments.Find(dptId);
            var dptName = "";
            if (dpt == null)
            {
                return Json(dptName);
            }
            dptName = dpt.Name_C;
            return Json(dptName);
        }

        [HttpPost]
        public JsonResult GetAssetName(string assetNo)
        {
            var asset = _context.BMEDAssets.Where(a => a.AssetNo == assetNo).FirstOrDefault();   
            if (asset == null)
            {
                return Json("查無資料");
            }
            var returnAsset = new
            {
                AssetNo = asset.AssetNo,
                Cname = asset.Cname,
                AccDate = asset.AccDate == null ? "" : asset.AccDate.Value.ToString("yyyy-MM-dd")
            };
            return Json(returnAsset);
        }

        public JsonResult GetAllEngs()
        {
            /* Get all engineers by role. */
            var allEngs = roleManager.GetUsersInRole("MedEngineer").ToList();
            List<AppUserModel> list = new List<AppUserModel>();
            foreach (string l in allEngs)
            {
                var u = _context.AppUsers.Where(a => a.UserName == l).FirstOrDefault();
                if (u != null)
                {
                    list.Add(u);
                }
            }
            list = list.OrderBy(l => l.DptId).ToList();
            return Json(list);
        }

        [HttpPost]
        public JsonResult GetAssetEngId(string AssetNo)
        {
            AssetModel asset = _context.BMEDAssets.Find(AssetNo);
            var engineer = _context.AppUsers.Find(asset.AssetEngId);

            /* 擷取預設負責工程師 */
            if (engineer == null)  //該部門無預設工程師，設定選取ID為99999的User，為尚未分配之案件
            {
                var tempEng = new { EngId = "0", UserName = "00000", FullName = "主管再行分派" };
                return Json(tempEng);
            }
            else
            {
                var eng = new { EngId = engineer.Id, UserName = engineer.UserName, FullName = engineer.FullName };

                return Json(eng);
            }
        }

        [HttpPost]
        public JsonResult GetAssetDeliver(string AssetNo)
        {
            AssetModel asset = _context.BMEDAssets.Find(AssetNo);
            AppUserModel u;
            List<SelectListItem> list = new List<SelectListItem>();

            if (asset != null)
            {
                if (asset.DelivUid != null)
                {
                    u = _context.AppUsers.Find(asset.DelivUid);
                    list.Add(new SelectListItem { Text = u.FullName + "(" + u.UserName + ")", Value = u.Id.ToString() });
                }
            }

            return Json(list);
        }

        public JsonResult GetRepairCounts()
        {
            /* Get user details. */
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            var repairCount = _context.BMEDRepairFlows.Where(f => f.Status == "?")
                                                      .Where(f => f.UserId == ur.Id).Count();
            return Json(repairCount);
        }

        public JsonResult QueryUsers(string QueryStr)
        {
            /* Search user by fullname or username. */
            var users = _context.AppUsers.Where(u => u.FullName.Contains(QueryStr) || 
                                                     u.UserName.Contains(QueryStr)).ToList();
            List<SelectListItem> list = new List<SelectListItem>();
            if(users.Count() != 0)
            {
                users.ForEach(ur => {
                    list.Add(new SelectListItem { Text = ur.FullName + "(" + ur.UserName + ")",
                                                  Value = ur.Id.ToString() });
                });
            }
            return Json(list);
        }

        public JsonResult QueryUsers2(string QueryStr, string QueryDptId)
        {
            var users = _context.AppUsers.ToList();

            /* Search user by fullname or username. */
            if (!string.IsNullOrEmpty(QueryStr))
            {
                users = users.Where(u => u.FullName.Contains(QueryStr) || u.UserName.Contains(QueryStr)).ToList();
            }

            /* Search user by department. */
            if (!string.IsNullOrEmpty(QueryDptId))
            {
                users = users.Where(u => u.DptId == QueryDptId).ToList();
            }

            /* If no search value. */
            if (string.IsNullOrEmpty(QueryDptId) && string.IsNullOrEmpty(QueryStr))
            {
                users = users.Where(u => u.FullName.Contains(QueryStr) || u.UserName.Contains(QueryStr)).ToList();
            }

            List<SelectListItem> list = new List<SelectListItem>();
            if (users.Count() != 0)
            {
                users.ForEach(ur => {
                    list.Add(new SelectListItem
                    {
                        Text = ur.FullName + "(" + ur.UserName + ")",
                        Value = ur.Id.ToString()
                    });
                });
            }
            return Json(list);
        }

        public JsonResult QueryAssets(string QueryStr, string QueryAccDpt, string QueryDelivDpt)
        {
            List<AssetModel> assets = new List<AssetModel>();
            // No query string.
            if (string.IsNullOrEmpty(QueryStr) && string.IsNullOrEmpty(QueryAccDpt) && string.IsNullOrEmpty(QueryDelivDpt))
            {
                assets = _context.BMEDAssets.Where(a => a.AssetNo.Contains(QueryStr) ||
                                                        a.Cname.Contains(QueryStr)).ToList();
            }
            else
            {
                assets = _context.BMEDAssets.ToList();
                if (!string.IsNullOrEmpty(QueryStr))     /* Search assets by assetNo or Cname. */
                {
                    assets = assets.Where(a => a.AssetNo.Contains(QueryStr) ||
                                               a.Cname.Contains(QueryStr)).ToList();
                }
                if (!string.IsNullOrEmpty(QueryAccDpt))    /* Search assets by AccDpt. */
                {
                    assets = assets.Where(a => a.AccDpt == QueryAccDpt).ToList();
                }
                if (!string.IsNullOrEmpty(QueryDelivDpt))   /* Search assets by DelivDpt. */
                {
                    assets = assets.Where(a => a.DelivDpt == QueryDelivDpt).ToList();
                }
            }

            List<SelectListItem> list = new List<SelectListItem>();
            if (assets.Count() != 0)
            {
                assets.ForEach(asset => {
                    list.Add(new SelectListItem
                    {
                        Text = asset.Cname + "(" + asset.AssetNo + ")",
                        Value = asset.AssetNo.ToString()
                    });
                });
            }
            return Json(list);
        }

        // POST: BMED/Repair/UpdateChecker
        [HttpPost]
        public IActionResult UpdateChecker(string DocId, string UpdChecker)
        {
            var repair = _context.BMEDRepairs.Find(DocId);
            if (repair == null)
            {
                return BadRequest();
            }
            repair.CheckerId = Convert.ToInt32(UpdChecker);
            _context.Entry(repair).State = EntityState.Modified;
            _context.SaveChanges();

            return new JsonResult(repair)
            {
                Value = new { success = true, error = "" }
            };
        }

        // GET: Repair/Delete/5
        public ActionResult Delete(string id)
        {
            // Find document.
            RepairModel repair = _context.BMEDRepairs.Find(id);
            repair.DptName = _context.Departments.Find(repair.DptId).Name_C;
            repair.AccDptName = _context.Departments.Find(repair.AccDpt).Name_C;
            repair.EngName = _context.AppUsers.Find(repair.EngId).FullName;
            repair.UserAccount = _context.AppUsers.Find(repair.UserId).UserName;

            if (repair == null)
            {
                return StatusCode(404);
            }
            return View(repair);
        }

        // GET: Repairs/PrintRepairDoc/5
        public IActionResult PrintRepairDoc(string DocId, int printType)
        {
            /* Get all print details according to the DocId. */
            RepairModel repair = _context.BMEDRepairs.Find(DocId);
            RepairDtlModel dtl = _context.BMEDRepairDtls.Find(DocId);
            RepairEmpModel emp = _context.BMEDRepairEmps.Where(ep => ep.DocId == DocId).FirstOrDefault();

            /* Get the last flow. */
            string[] s = new string[] { "?", "2" };
            RepairFlowModel flow = _context.BMEDRepairFlows.Where(f => f.DocId == DocId)
                                                           .Where(f => s.Contains(f.Status)).FirstOrDefault();
            RepairPrintVModel vm = new RepairPrintVModel();
            if (repair == null)
            {
                return StatusCode(404);
            }
            else
            {
                vm.Docid = DocId;
                vm.UserId = repair.UserId;
                vm.UserName = repair.UserName;
                vm.UserAccount = _context.AppUsers.Find(repair.UserId).UserName;
                vm.AccDpt = repair.AccDpt;
                vm.ApplyDate = repair.ApplyDate;
                vm.AssetNo = repair.AssetNo;
                vm.AssetNam = repair.AssetName;
                vm.Company = "(" + _context.Departments.Find(repair.DptId).DptId + ")" + _context.Departments.Find(repair.DptId).Name_C;
                vm.Contact = repair.Ext;
                vm.MVPN = repair.Mvpn;
                vm.Amt = repair.Amt;
                vm.PlantDoc = repair.PlantDoc;
                vm.PlaceLoc = repair.PlaceLoc;
                vm.RepType = repair.RepType;
                vm.TroubleDes = repair.TroubleDes;
                if (dtl != null)
                {
                    vm.DealDes = dtl.DealDes;
                    vm.EndDate = dtl.EndDate;
                }
                //
                vm.AccDptNam = _context.Departments.Find(repair.AccDpt).Name_C;
                vm.Hour = dtl.Hour;
                vm.InOut = dtl.InOut;
                //vm.EngName = emp == null ? "" : _context.AppUsers.Find(emp.UserId).FullName;
                var lastFlowEng = _context.BMEDRepairFlows.Where(rf => rf.DocId == DocId)
                                                          .Where(rf => rf.Cls.Contains("工程師"))
                                                          .OrderByDescending(rf => rf.StepId).FirstOrDefault();
                AppUserModel EngTemp = _context.AppUsers.Find(lastFlowEng.UserId);       
                if (EngTemp != null)
                {
                    vm.EngName = " (" + EngTemp.UserName + ")" + EngTemp.FullName;
                }
                else
                {
                    vm.EngName = "";
                }
                var engMgr = _context.BMEDRepairFlows.Where(r => r.DocId == DocId)
                                                     .Where(r => r.Cls.Contains("醫工主管")).ToList();
                if(engMgr.Count() != 0)
                {
                    engMgr = engMgr.GroupBy(e => e.UserId).Select(group => group.FirstOrDefault()).ToList();
                    foreach (var item in engMgr)
                    {
                        vm.EngMgr += item == null ? "" : _context.AppUsers.Find(item.UserId).FullName + "  ";
                    }
                }

                var engDirector = _context.BMEDRepairFlows.Where(r => r.DocId == DocId)
                                                          .Where(r => r.Cls.Contains("醫工主任")).LastOrDefault();
                vm.EngDirector = engDirector == null ? "" : _context.AppUsers.Find(engDirector.UserId).FullName;

                var delivMgr = _context.BMEDRepairFlows.Where(r => r.DocId == DocId)
                                                       .Where(r => r.Cls.Contains("單位主管")).ToList();
                if (delivMgr.Count() != 0)
                {
                    delivMgr = delivMgr.GroupBy(e => e.UserId).Select(group => group.FirstOrDefault()).ToList();
                    foreach (var item in delivMgr)
                    {
                        vm.DelivMgr += item == null ? "" : _context.AppUsers.Find(item.UserId).FullName + "  ";
                    }
                }

                var delivDirector = _context.BMEDRepairFlows.Where(r => r.DocId == DocId)
                                                            .Where(r => r.Cls.Contains("單位主任")).LastOrDefault();
                vm.DelivDirector = delivDirector == null ? "" : _context.AppUsers.Find(delivDirector.UserId).FullName;

                var ViceSI = _context.BMEDRepairFlows.Where(r => r.DocId == DocId)
                                                     .Where(r => r.Cls.Contains("副院長")).LastOrDefault();
                vm.ViceSuperintendent = ViceSI == null ? "" : _context.AppUsers.Find(ViceSI.UserId).FullName;

                if (flow != null)
                {
                    if (flow.Status == "2")
                    {
                        vm.CloseDate = flow.Rtt;
                        AppUserModel u = _context.AppUsers.Find(flow.UserId);
                        if (u != null)
                        {
                            vm.DelivEmp = u.UserName;
                            vm.DelivEmpName = u.FullName;
                        }
                    }
                }

                /* Draw barcode and use Base64 to image to show barcode on view. */
                var barcodeString = repair.DocId.ToString();
                Zen.Barcode.Code128BarcodeDraw barcode1 = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                var barcodeImage = barcode1.Draw(barcodeString, 30);
                using (MemoryStream ms = new MemoryStream())
                {
                    barcodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();

                    ViewBag.Img = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
                }

            }
            //if( printType != 0 )
            //{
            //    return View("PrintRepairDoc2", vm);
            //}
            return View(vm);
        }

        public IActionResult ExportToExcel(string qtyDocId, string qtyAssetNo, string qtyAccDpt, string qtyAssetName,
                                           string qtyFlowType, string qtyDptId, string Date1, string Date2,
                                           string DealStatus, string IsCharged, string DateType, bool SearchAllDoc,
                                           string EngCode, string TicketNo, string Vendor)
        {
            string docid = qtyDocId;
            string ano = qtyAssetNo;
            string acc = qtyAccDpt;
            string aname = qtyAssetName;
            string ftype = qtyFlowType;
            string dptid = qtyDptId;
            string qtyDate1 = Date1;
            string qtyDate2 = Date2;
            string qtyDealStatus = DealStatus;
            string qtyIsCharged = IsCharged;
            string qtyDateType = DateType;
            bool searchAllDoc = SearchAllDoc;
            string qtyEngCode = EngCode;
            string qtyTicketNo = TicketNo;
            string qtyVendor = Vendor;

            DateTime applyDateFrom = DateTime.Now;
            DateTime applyDateTo = DateTime.Now;
            /* Dealing search by date. */
            if (qtyDate1 != null && qtyDate2 != null)// If 2 date inputs have been insert, compare 2 dates.
            {
                DateTime date1 = DateTime.Parse(qtyDate1);
                DateTime date2 = DateTime.Parse(qtyDate2);
                int result = DateTime.Compare(date1, date2);
                if (result < 0)
                {
                    applyDateFrom = date1.Date;
                    applyDateTo = date2.Date;
                }
                else if (result == 0)
                {
                    applyDateFrom = date1.Date;
                    applyDateTo = date1.Date;
                }
                else
                {
                    applyDateFrom = date2.Date;
                    applyDateTo = date1.Date;
                }
            }
            else if (qtyDate1 == null && qtyDate2 != null)
            {
                applyDateFrom = DateTime.Parse(qtyDate2);
                applyDateTo = DateTime.Parse(qtyDate2);
            }
            else if (qtyDate1 != null && qtyDate2 == null)
            {
                applyDateFrom = DateTime.Parse(qtyDate1);
                applyDateTo = DateTime.Parse(qtyDate1);
            }


            List<RepairListVModel> rv = new List<RepairListVModel>();
            /* Get login user. */
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();

            var rps = _context.BMEDRepairs.ToList();

            if (!string.IsNullOrEmpty(docid))   //表單編號
            {
                docid = docid.Trim();
                rps = rps.Where(v => v.DocId == docid).ToList();
            }
            if (!string.IsNullOrEmpty(ano))     //財產編號
            {
                rps = rps.Where(v => v.AssetNo == ano).ToList();
            }
            if (!string.IsNullOrEmpty(dptid))   //所屬部門編號
            {
                rps = rps.Where(v => v.DptId == dptid).ToList();
            }
            if (!string.IsNullOrEmpty(acc))     //成本中心
            {
                rps = rps.Where(v => v.AccDpt == acc).ToList();
            }
            if (!string.IsNullOrEmpty(aname))   //財產名稱
            {
                rps = rps.Where(v => v.AssetName != null)
                         .Where(v => v.AssetName.Contains(aname))
                         .ToList();
            }
            if (!string.IsNullOrEmpty(qtyTicketNo))     //發票號碼
            {
                qtyTicketNo = qtyTicketNo.ToUpper();
                var resultDocIds = _context.BMEDRepairCosts.Include(rc => rc.TicketDtl)
                                                           .Where(rc => rc.TicketDtl.TicketDtlNo == qtyTicketNo)
                                                           .Select(rc => rc.DocId).Distinct();
                rps = (from r in rps
                       where resultDocIds.Any(val => r.DocId.Contains(val))
                       select r).ToList();
            }
            if (!string.IsNullOrEmpty(qtyVendor))   //廠商關鍵字
            {
                var resultDocIds = _context.BMEDRepairCosts.Include(rc => rc.TicketDtl)
                                                           .Where(rc => rc.VendorName.Contains(qtyVendor))
                                                           .Select(rc => rc.DocId).Distinct();
                rps = (from r in rps
                       where resultDocIds.Any(val => r.DocId.Contains(val))
                       select r).ToList();
            }
            /* Search date by DateType.(ApplyDate) */
            if (string.IsNullOrEmpty(qtyDate1) == false || string.IsNullOrEmpty(qtyDate2) == false)
            {
                if (qtyDateType == "申請日")
                {
                    rps = rps.Where(v => v.ApplyDate >= applyDateFrom && v.ApplyDate <= applyDateTo).ToList();
                }
            }

            switch (ftype)
            {
                /* 與登入者相關且流程不在該登入者身上的文件 */
                case "流程中":
                    rps.Join(_context.BMEDRepairFlows.Where(f2 => f2.UserId == ur.Id && f2.Status == "1")
                       .Select(f => f.DocId).Distinct(),
                               r => r.DocId, f2 => f2, (r, f2) => r)
                       .Join(_context.BMEDRepairFlows.Where(f => f.Status == "?" && f.UserId != ur.Id),
                       r => r.DocId, f => f.DocId,
                       (r, f) => new
                       {
                           repair = r,
                           flow = f
                       })
                       .Join(_context.BMEDRepairDtls, m => m.repair.DocId, d => d.DocId,
                       (m, d) => new
                       {
                           repair = m.repair,
                           flow = m.flow,
                           repdtl = d
                       })
                       .Join(_context.Departments, j => j.repair.AccDpt, d => d.DptId,
                       (j, d) => new
                       {
                           repair = j.repair,
                           flow = j.flow,
                           repdtl = j.repdtl,
                           dpt = d
                       })
                       .ToList()
                       .ForEach(j => rv.Add(new RepairListVModel
                       {
                           DocType = "醫工請修",
                           RepType = j.repair.RepType,
                           DocId = j.repair.DocId,
                           ApplyDate = j.repair.ApplyDate,
                           PlaceLoc = j.repair.PlaceLoc,
                           ApplyDpt = j.repair.DptId,
                           AccDpt = j.repair.AccDpt,
                           AccDptName = j.dpt.Name_C,
                           TroubleDes = j.repair.TroubleDes,
                           DealState = _context.BMEDDealStatuses.Find(j.repdtl.DealState).Title,
                           DealDes = j.repdtl.DealDes,
                           Cost = j.repdtl.Cost,
                           Days = DateTime.Now.Subtract(j.repair.ApplyDate).Days,
                           Flg = j.flow.Status,
                           FlowUid = j.flow.UserId,
                           FlowCls = j.flow.Cls,
                           FlowDptId = _context.AppUsers.Find(j.flow.UserId).DptId,
                           EndDate = j.repdtl.EndDate,
                           CloseDate = j.repdtl.CloseDate,
                           IsCharged = j.repdtl.IsCharged,
                           repdata = j.repair
                       }));
                    break;
                /* 與登入者相關且結案的文件 */
                case "已結案":
                    /* Get all closed repair docs. */
                    List<RepairFlowModel> rf = _context.BMEDRepairFlows.Where(f => f.Status == "2").ToList();

                    if (userManager.IsInRole(User, "Admin") || userManager.IsInRole(User, "MedAdmin") || 
                        userManager.IsInRole(User, "Manager") || userManager.IsInRole(User, "MedEngineer"))
                    {
                        if (userManager.IsInRole(User, "Manager"))
                        {
                            rf = rf.Join(_context.BMEDRepairs.Where(r => r.AccDpt == ur.DptId),
                            f => f.DocId, r => r.DocId, (f, r) => f).ToList();
                        }
                        /* If no other search values, search the docs belong the login engineer. */
                        if (userManager.IsInRole(User, "MedEngineer") && searchAllDoc == false)
                        {
                            rf = rf.Join(_context.BMEDRepairFlows.Where(f2 => f2.UserId == ur.Id),
                                 f => f.DocId, f2 => f2.DocId, (f, f2) => f).ToList();
                        }
                    }
                    else /* If normal user, search the docs belong himself. */
                    {
                        rf = rf.Join(_context.BMEDRepairFlows.Where(f2 => f2.UserId == ur.Id),
                             f => f.DocId, f2 => f2.DocId, (f, f2) => f).ToList();
                    }

                    rf.Select(f => new
                    {
                        f.DocId,
                        f.UserId,
                        f.Status,
                        f.Cls
                    }).Distinct().Join(rps.DefaultIfEmpty(), f => f.DocId, r => r.DocId,
                    (f, r) => new
                    {
                        repair = r,
                        flow = f
                    })
                    .Join(_context.BMEDRepairDtls, m => m.repair.DocId, d => d.DocId,
                    (m, d) => new
                    {
                        repair = m.repair,
                        flow = m.flow,
                        repdtl = d
                    })
                    .Join(_context.Departments, j => j.repair.AccDpt, d => d.DptId,
                    (j, d) => new
                    {
                        repair = j.repair,
                        flow = j.flow,
                        repdtl = j.repdtl,
                        dpt = d
                    }).ToList()
                    .ForEach(j => rv.Add(new RepairListVModel
                    {
                        DocType = "醫工請修",
                        RepType = j.repair.RepType,
                        DocId = j.repair.DocId,
                        ApplyDate = j.repair.ApplyDate,
                        PlaceLoc = j.repair.PlaceLoc,
                        ApplyDpt = j.repair.DptId,
                        AccDpt = j.repair.AccDpt,
                        AccDptName = j.dpt.Name_C,
                        TroubleDes = j.repair.TroubleDes,
                        DealState = _context.BMEDDealStatuses.Find(j.repdtl.DealState).Title,
                        DealDes = j.repdtl.DealDes,
                        Cost = j.repdtl.Cost,
                        Days = DateTime.Now.Subtract(j.repair.ApplyDate).Days,
                        Flg = j.flow.Status,
                        FlowUid = j.flow.UserId,
                        FlowCls = j.flow.Cls,
                        FlowDptId = _context.AppUsers.Find(j.flow.UserId).DptId,
                        EndDate = j.repdtl.EndDate,
                        CloseDate = j.repdtl.CloseDate,
                        IsCharged = j.repdtl.IsCharged,
                        repdata = j.repair
                    }));
                    break;
                /* 與登入者相關且流程在該登入者身上的文件 */
                case "待簽核":
                    /* Get all dealing repair docs. */
                    var repairFlows = _context.BMEDRepairFlows.Join(rps.DefaultIfEmpty(), f => f.DocId, r => r.DocId,
                    (f, r) => new
                    {
                        repair = r,
                        flow = f
                    }).ToList();

                    if (userManager.IsInRole(User, "Admin") || userManager.IsInRole(User, "MedAdmin") ||
                        userManager.IsInRole(User, "MedEngineer"))
                    {
                        /* If has other search values, search all RepairDocs which flowCls is in engineer. */
                        /* Else return the docs belong the login engineer.  */
                        if (userManager.IsInRole(User, "MedEngineer") && searchAllDoc == true)
                        {
                            repairFlows = repairFlows.Where(f => f.flow.Status == "?" && f.flow.Cls.Contains("工程師")).ToList();
                            if (!string.IsNullOrEmpty(qtyEngCode))  //工程師搜尋
                            {
                                repairFlows = repairFlows.Where(f => f.repair.EngId == Convert.ToInt32(qtyEngCode)).ToList();
                            }
                        }
                        else
                        {
                            /* 個人或同部門結案案件 */
                            //repairFlows = repairFlows.Where(f => (f.flow.Status == "?" && f.flow.UserId == ur.Id) ||
                            //                                     (f.flow.Status == "?" && f.flow.Cls == "驗收人" &&
                            //                                      _context.AppUsers.Find(f.flow.UserId).DptId == ur.DptId)).ToList();

                            /* 個人案件 */
                            repairFlows = repairFlows.Where(f => (f.flow.Status == "?" && f.flow.UserId == ur.Id)).ToList();
                        }
                    }
                    else
                    {
                        /* 個人或同部門結案案件 */
                        //repairFlows = repairFlows.Where(f => (f.flow.Status == "?" && f.flow.UserId == ur.Id) ||
                        //                                     (f.flow.Status == "?" && f.flow.Cls == "驗收人" &&
                        //                                       _context.AppUsers.Find(f.flow.UserId).DptId == ur.DptId)).ToList();

                        /* 個人案件 */
                        repairFlows = repairFlows.Where(f => (f.flow.Status == "?" && f.flow.UserId == ur.Id)).ToList();
                    }

                    repairFlows.Join(_context.BMEDRepairDtls, m => m.repair.DocId, d => d.DocId,
                    (m, d) => new
                    {
                        repair = m.repair,
                        flow = m.flow,
                        repdtl = d
                    })
                    .Join(_context.Departments, j => j.repair.AccDpt, d => d.DptId,
                    (j, d) => new
                    {
                        repair = j.repair,
                        flow = j.flow,
                        repdtl = j.repdtl,
                        dpt = d
                    }).ToList()
                    .ForEach(j => rv.Add(new RepairListVModel
                    {
                        DocType = "醫工請修",
                        RepType = j.repair.RepType,
                        DocId = j.repair.DocId,
                        ApplyDate = j.repair.ApplyDate,
                        PlaceLoc = j.repair.PlaceLoc,
                        ApplyDpt = j.repair.DptId,
                        AccDpt = j.repair.AccDpt,
                        AccDptName = j.dpt.Name_C,
                        TroubleDes = j.repair.TroubleDes,
                        DealState = _context.BMEDDealStatuses.Find(j.repdtl.DealState).Title,
                        DealDes = j.repdtl.DealDes,
                        Cost = j.repdtl.Cost,
                        Days = DateTime.Now.Subtract(j.repair.ApplyDate).Days,
                        Flg = j.flow.Status,
                        FlowUid = j.flow.UserId,
                        FlowCls = j.flow.Cls,
                        FlowDptId = _context.AppUsers.Find(j.flow.UserId).DptId,
                        EndDate = j.repdtl.EndDate,
                        CloseDate = j.repdtl.CloseDate,
                        IsCharged = j.repdtl.IsCharged,
                        repdata = j.repair
                    }));
                    break;
            };

            /* 設備編號"有"、"無"的對應，"有"讀取table相關data，"無"只顯示申請人輸入的設備名稱 */
            foreach (var item in rv)
            {
                if (!string.IsNullOrEmpty(item.repdata.AssetNo))
                {
                    var asset = _context.BMEDAssets.Where(a => a.AssetNo == item.repdata.AssetNo).FirstOrDefault();
                    if (asset != null)
                    {
                        item.AssetNo = asset.AssetNo;
                        item.AssetName = asset.Cname;
                        item.Brand = asset.Brand;
                        item.Type = asset.Type;
                    }
                }
                else
                {
                    item.AssetName = item.repdata.AssetName;
                }
            }

            /* Search date by DateType. */
            if (string.IsNullOrEmpty(qtyDate1) == false || string.IsNullOrEmpty(qtyDate2) == false)
            {
                if (qtyDateType == "結案日")
                {
                    rv = rv.Where(v => v.CloseDate >= applyDateFrom && v.CloseDate <= applyDateTo).ToList();
                }
                else if (qtyDateType == "完工日")
                {
                    rv = rv.Where(v => v.EndDate >= applyDateFrom && v.EndDate <= applyDateTo).ToList();
                }
            }

            /* Sorting search result. */
            if (rv.Count() != 0)
            {
                if (qtyDateType == "結案日")
                {
                    rv = rv.OrderByDescending(r => r.CloseDate).ThenByDescending(r => r.DocId).ToList();
                }
                else if (qtyDateType == "完工日")
                {
                    rv = rv.OrderByDescending(r => r.EndDate).ThenByDescending(r => r.DocId).ToList();
                }
                else
                {
                    rv = rv.OrderByDescending(r => r.ApplyDate).ThenByDescending(r => r.DocId).ToList();
                }
            }

            /* Search dealStatus. */
            if (!string.IsNullOrEmpty(qtyDealStatus))
            {
                rv = rv.Where(r => r.DealState == qtyDealStatus).ToList();
            }
            /* Search IsCharged. */
            if (!string.IsNullOrEmpty(qtyIsCharged))
            {
                rv = rv.Where(r => r.IsCharged == qtyIsCharged).ToList();
            }

            //ClosedXML的用法 先new一個Excel Workbook
            using (XLWorkbook workbook = new XLWorkbook())
            {
                //取得要塞入Excel內的資料
                var data = rv.Select(c => new {
                    c.RepType,
                    c.DocId,
                    c.ApplyDate,
                    AccDpt = c.AccDptName + "(" + c.AccDpt + ")",
                    Asset = c.AssetName + "(" + c.AssetNo + ")",
                    c.PlaceLoc,
                    Location = c.Location1 + c.Location2,
                    c.TroubleDes,
                    c.DealDes,
                    c.DealState,
                    c.EndDate,
                    c.CloseDate,
                    c.Cost,
                    c.Days,
                    c.FlowCls
                });

                //一個workbook內至少會有一個worksheet,並將資料Insert至這個位於A1這個位置上
                var ws = workbook.Worksheets.Add("sheet1", 1);

                //Title
                ws.Cell(1, 1).Value = "請修類別";
                ws.Cell(1, 2).Value = "表單編號";
                ws.Cell(1, 3).Value = "申請日期";
                ws.Cell(1, 4).Value = "成本中心";
                ws.Cell(1, 5).Value = "物品名稱(財產編號)";
                ws.Cell(1, 6).Value = "請修地點";
                ws.Cell(1, 7).Value = "請修地點(詳細位置)";
                ws.Cell(1, 8).Value = "故障描述";
                ws.Cell(1, 9).Value = "處理描述";
                ws.Cell(1, 10).Value = "處理狀態";
                ws.Cell(1, 11).Value = "完工日期";
                ws.Cell(1, 12).Value = "結案日期";
                ws.Cell(1, 13).Value = "費用";
                ws.Cell(1, 14).Value = "天數";
                ws.Cell(1, 15).Value = "關卡";

                //如果是要塞入Query後的資料該資料一定要變成是data.AsEnumerable()
                ws.Cell(2, 1).InsertData(data);

                //因為是用Query的方式,這個地方要用串流的方式來存檔
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    //請注意 一定要加入這行,不然Excel會是空檔
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    //注意Excel的ContentType,是要用這個"application/vnd.ms-excel"
                    string fileName = "案件搜尋_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx";
                    return this.File(memoryStream.ToArray(), "application/vnd.ms-excel", fileName);
                }
            }
        }

        // POST: Repairs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            RepairFlowModel repflow = _context.BMEDRepairFlows.Where(f => f.DocId == id && f.Status == "?")
                                                              .FirstOrDefault();
            repflow.Status = "3";
            repflow.Rtp = ur.Id;
            repflow.Rtt = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction("Index", "Home", new { Area = "" });
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
            base.Dispose(disposing);
        }
    }
}
