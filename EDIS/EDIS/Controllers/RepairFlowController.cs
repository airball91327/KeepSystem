using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EDIS.Data;
using EDIS.Models;
using EDIS.Models.Identity;
using EDIS.Models.LocationModels;
using EDIS.Models.RepairModels;
using EDIS.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EDIS.Controllers
{
    [Authorize]
    public class RepairFlowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<RepairModel, string> _repRepo;
        private readonly IRepository<RepairFlowModel, string[]> _repflowRepo;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;

        public RepairFlowController(ApplicationDbContext context,
                                    IRepository<RepairModel, string> repairRepo,
                                    IRepository<RepairFlowModel, string[]> repairflowRepo,
                                    IRepository<AppUserModel, int> userRepo,
                                    CustomUserManager customUserManager,
                                    CustomRoleManager customRoleManager)
        {
            _context = context;
            _repRepo = repairRepo;
            _repflowRepo = repairflowRepo;
            _userRepo = userRepo;
            userManager = customUserManager;
            roleManager = customRoleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NextFlow(AssignModel assign)
        {
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            
            /* 工程師的流程控管 */
            if(assign.Cls == "工務/營建工程師")
            {
                /* 如點選有費用、卻無輸入費用明細 */
                var isCharged = _context.RepairDtls.Where(d => d.DocId == assign.DocId).FirstOrDefault().IsCharged;
                if( isCharged == "Y" )
                {
                    var CheckRepairCost = _context.RepairCosts.Where(c => c.DocId == assign.DocId).FirstOrDefault();
                    if(CheckRepairCost == null)
                    {
                        string msg = "尚未輸入費用明細!!";
                        return BadRequest(msg);
                    }
                }
                var repairDtl = _context.RepairDtls.Where(d => d.DocId == assign.DocId).FirstOrDefault();
                /* 3 = 已完成，4 = 報廢 */
                if (repairDtl.DealState == 3 || repairDtl.DealState == 4)
                {
                    if(repairDtl.EndDate == null)
                    {
                        string msg = "報廢及已完成，需輸入完工日!!";
                        return BadRequest(msg);
                    }
                }
                /* 工程師做結案 */
                if (assign.FlowCls == "結案")
                {
                    if (_context.RepairEmps.Where(emp => emp.DocId == assign.DocId).Count() <= 0)
                    {
                        string msg = "沒有維修工程師紀錄!!";
                        return BadRequest(msg);
                    }
                    else if (_context.RepairDtls.Find(assign.DocId).EndDate == null)
                    {
                        string msg = "沒有完工日!!";
                        return BadRequest(msg);
                    }
                    else if (_context.RepairDtls.Find(assign.DocId).DealState == 0)
                    {
                        string msg = "處理狀態不可空值!!";
                        return BadRequest(msg);
                    }
                    if (_context.RepairDtls.Find(assign.DocId).FailFactor == 0)
                    {
                        string msg = "故障原因不可空白!!";
                        return BadRequest(msg);
                    }
                    if (string.IsNullOrEmpty(_context.RepairDtls.Find(assign.DocId).InOut))
                    {
                        string msg = "維修方式不可空白!!";
                        return BadRequest(msg);
                    }
                    if (_context.RepairDtls.Find(assign.DocId).DealState == 1 || _context.RepairDtls.Find(assign.DocId).DealState == 2)
                    {
                        string msg = "處理狀態不可為處理中或未處理!!";
                        return BadRequest(msg);
                    }
                }
            }

            if (assign.FlowCls == "結案" || assign.FlowCls == "廢除")
                assign.FlowUid = ur.Id;
            if (ModelState.IsValid)
            {
                RepairFlowModel rf = _context.RepairFlows.Where(f => f.DocId == assign.DocId && f.Status == "?").FirstOrDefault();
                if (assign.FlowCls == "驗收人")
                {
                    if (_context.RepairEmps.Where(emp => emp.DocId == assign.DocId).Count() <= 0)
                    {
                        //throw new Exception("沒有維修工程師紀錄!!");
                        string msg = "沒有維修工程師紀錄!!";
                        return BadRequest(msg);
                    }
                    else if (_context.RepairDtls.Find(assign.DocId).EndDate == null)
                    {
                        //throw new Exception("沒有完工日!!");
                        string msg = "沒有完工日!!";
                        return BadRequest(msg);
                    }
                    else if (_context.RepairDtls.Find(assign.DocId).DealState == 0)
                    {
                        //throw new Exception("處理狀態不可空值!!");
                        string msg = "處理狀態不可空值!!";
                        return BadRequest(msg);
                    }
                    if (_context.RepairDtls.Find(assign.DocId).FailFactor == 0)
                    {
                        //throw new Exception("故障原因不可空白!!");
                        string msg = "故障原因不可空白!!";
                        return BadRequest(msg);
                    }
                    if (string.IsNullOrEmpty(_context.RepairDtls.Find(assign.DocId).InOut))
                    {
                        //throw new Exception("維修方式不可空白!!");
                        string msg = "維修方式不可空白!!";
                        return BadRequest(msg);
                    }
                    if (_context.RepairDtls.Find(assign.DocId).DealState == 1 || _context.RepairDtls.Find(assign.DocId).DealState == 2)
                    {
                        //throw new Exception("維修方式不可空白!!");
                        string msg = "處理狀態不可為處理中或未處理!!";
                        return BadRequest(msg);
                    }
                }
                if (assign.FlowCls == "結案")
                {
                    RepairDtlModel rd = _context.RepairDtls.Find(assign.DocId);
                    rd.CloseDate = DateTime.Now;
                    rf.Opinions = "[" + assign.AssignCls + "]" + Environment.NewLine + assign.AssignOpn;
                    rf.Status = "2";
                    rf.UserId = ur.Id;
                    rf.UserName = _context.AppUsers.Find(ur.Id).FullName;
                    rf.Rtt = DateTime.Now;
                    rf.Rtp = ur.Id;
                    _context.Entry(rf).State = EntityState.Modified;
                    _context.Entry(rd).State = EntityState.Modified;

                    //If "結案", delete 1 dealing doc to the engineer.
                    //var repairDoc = _context.Repairs.Find(assign.DocId);
                    //var eng = _context.EngDealingDocs.Find(repairDoc.EngId);
                    //eng.DealingDocs = eng.DealingDocs - 1;
                    //_context.Entry(eng).State = EntityState.Modified;
                    _context.SaveChanges();

                    //Send Mail
                    //To all users in this repair's flow.
                    //try
                    //{
                    //    Tmail mail = new Tmail();
                    //    string body = "";
                    //    string sto = "";
                    //    AppUserModel u;
                    //    RepairModel repair = _context.Repairs.Find(assign.DocId);
                    //    if (repair.Building != null)
                    //    {
                    //        repair.BuildingName = _context.Buildings.Where(b => b.BuildingId == Convert.ToInt32(repair.Building)).FirstOrDefault().BuildingName;
                    //        repair.FloorName = _context.Floors.Where(f => f.BuildingId == Convert.ToInt32(repair.Building) && f.FloorId == repair.Floor).FirstOrDefault().FloorName;
                    //        repair.AreaName = _context.Places.Where(p => p.BuildingId == Convert.ToInt32(repair.Building) && p.FloorId == repair.Floor && p.PlaceId == repair.Area).FirstOrDefault().PlaceName;
                    //    }
                    //    else
                    //    {
                    //        repair.BuildingName = "(無資料)";
                    //        repair.FloorName = "";
                    //        repair.AreaName = "";
                    //    }
                    //    mail.from = new System.Net.Mail.MailAddress(ur.Email); //u.Email
                    //    /* If is charged, send mail to all flow users. */
                    //    if (rd.IsCharged == "Y")
                    //    {
                    //        _context.RepairFlows.Where(f => f.DocId == assign.DocId)
                    //            .ToList()
                    //            .ForEach(f =>
                    //            {
                    //                u = _context.AppUsers.Find(f.UserId);
                    //                sto += u.Email + ",";
                    //            });
                    //    }
                    //    else
                    //    {
                    //        _context.RepairFlows.Where(f => f.DocId == assign.DocId).Where(f => f.Cls.Contains("工程師") == false)
                    //            .ToList()
                    //            .ForEach(f =>
                    //            {
                    //                u = _context.AppUsers.Find(f.UserId);
                    //                sto += u.Email + ",";
                    //            });
                    //    }
                    //    var temp = _context.RepairFlows.Where(f => f.DocId == assign.DocId).Where(f => f.Cls.Contains("工程師") == false)
                    //            .ToList();
                    //    mail.sto = sto.TrimEnd(new char[] { ',' });

                    //    mail.message.Subject = "工務智能請修系統[請修案-結案通知]：設備名稱： " + repair.AssetName;
                    //    body += "<p>表單編號：" + repair.DocId + "</p>";
                    //    body += "<p>申請日期：" + repair.ApplyDate.ToString("yyyy/MM/dd") + "</p>";
                    //    body += "<p>申請人：" + repair.UserName + "</p>";
                    //    body += "<p>財產編號：" + repair.AssetNo + "</p>";
                    //    body += "<p>設備名稱：" + repair.AssetName + "</p>";
                    //    body += "<p>請修地點：" + repair.PlaceLoc + " " + repair.BuildingName + " " + repair.FloorName + " " + repair.AreaName + "</p>";
                    //    //body += "<p>放置地點：" + repair.PlaceLoc + "</p>";
                    //    body += "<p>故障描述：" + repair.TroubleDes + "</p>";
                    //    body += "<p>處理描述：" + rd.DealDes + "</p>";
                    //    body += "<p><a href='http://dms.cch.org.tw/EDIS/Account/Login" + "?DocId=" + repair.DocId + "&dealType=Views'" + ">檢視案件</a></p>";
                    //    body += "<br/>";
                    //    body += "<p>使用ＩＥ瀏覽器注意事項：</p>";
                    //    body += "<p>「工具」→「相容性檢視設定」→移除cch.org.tw</p>";
                    //    body += "<br/>";
                    //    body += "<h3>此封信件為系統通知郵件，請勿回覆。</h3>";
                    //    body += "<br/>";
                    //    body += "<h3 style='color:red'>如有任何疑問請聯絡工務部，分機3033或7033。<h3>";
                    //    mail.message.Body = body;
                    //    mail.message.IsBodyHtml = true;
                    //    mail.SendMail();
                    //}
                    //catch (Exception ex)
                    //{
                    //    throw new Exception(ex.Message);
                    //}
                }
                else if (assign.FlowCls == "廢除")
                {
                    rf.Opinions = "[廢除]" + Environment.NewLine + assign.AssignOpn;
                    rf.Status = "3";
                    rf.Rtt = DateTime.Now;
                    rf.Rtp = ur.Id;
                    _context.Entry(rf).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                else
                {
                    //轉送下一關卡
                    rf.Opinions = "[" + assign.AssignCls + "]" + Environment.NewLine + assign.AssignOpn;
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

                    //Send Mail
                    //To the next flow user.
                    try
                    {
                        Tmail mail = new Tmail();
                        string body = "";
                        AppUserModel u;
                        RepairModel repair = _context.Repairs.Find(assign.DocId);
                        if(repair.Building != null)
                        {
                            repair.BuildingName = _context.Buildings.Where(b => b.BuildingId == Convert.ToInt32(repair.Building)).FirstOrDefault().BuildingName;
                            repair.FloorName = _context.Floors.Where(f => f.BuildingId == Convert.ToInt32(repair.Building) && f.FloorId == repair.Floor).FirstOrDefault().FloorName;
                            repair.AreaName = _context.Places.Where(p => p.BuildingId == Convert.ToInt32(repair.Building) && p.FloorId == repair.Floor && p.PlaceId == repair.Area).FirstOrDefault().PlaceName;
                        }
                        else
                        {
                            repair.BuildingName = "(無資料)";
                            repair.FloorName = "";
                            repair.AreaName = "";
                        }
                        mail.from = new System.Net.Mail.MailAddress(ur.Email); //ur.Email
                        u = _context.AppUsers.Find(flow.UserId);
                        mail.to = new System.Net.Mail.MailAddress(u.Email); //u.Email
                                                                            //mail.cc = new System.Net.Mail.MailAddress("99242@cch.org.tw");
                        mail.message.Subject = "工務智能請修系統[請修案]：設備名稱： " + repair.AssetName;
                        body += "<p>表單編號：" + repair.DocId + "</p>";
                        body += "<p>申請日期：" + repair.ApplyDate.ToString("yyyy/MM/dd") + "</p>";
                        body += "<p>申請人：" + repair.UserName + "</p>";
                        body += "<p>財產編號：" + repair.AssetNo + "</p>";
                        body += "<p>設備名稱：" + repair.AssetName + "</p>";
                        body += "<p>故障描述：" + repair.TroubleDes + "</p>";
                        body += "<p>請修地點：" + repair.PlaceLoc + " " + repair.BuildingName + " " + repair.FloorName + " " + repair.AreaName + "</p>";
                        //body += "<p>放置地點：" + repair.PlaceLoc + "</p>";
                        body += "<p><a href='http://dms.cch.org.tw/EDIS/Account/Login" + "?docId=" + repair.DocId + "&dealType=Edit'" + ">處理案件</a></p>";
                        body += "<br/>";
                        body += "<p>使用ＩＥ瀏覽器注意事項：</p>";
                        body += "<p>「工具」→「相容性檢視設定」→移除cch.org.tw</p>";
                        body += "<br/>";
                        body += "<h3>此封信件為系統通知郵件，請勿回覆。</h3>";
                        body += "<br/>";
                        body += "<h3 style='color:red'>如有任何疑問請聯絡工務部，分機3033或7033。<h3>";
                        mail.message.Body = body;
                        mail.message.IsBodyHtml = true;
                        /* If next flow is not engineer, send mail. */
                        if (flow.Cls.Contains("工程師") == false)
                        {
                            mail.SendMail();
                        }                   
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }

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

        [HttpPost]
        public ActionResult GetNextEmp(string cls, string docid/*, string vendor*/)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            List<string> s;
            SelectListItem li;
            AppUserModel u;
            RepairModel r = _context.Repairs.Find(docid);
            AssetModel asset = _context.Assets.Where(a => a.AssetNo == r.AssetNo).FirstOrDefault();

            switch (cls)
            {
                //case "維修工程師":
                //    roleManager.GetUsersInRole("Engineer").ToList()
                //        .ForEach(x =>
                //        {
                //            u = _context.AppUsers.Where(ur => ur.UserName == x).FirstOrDefault();
                //            if (vendor != null && u != null)
                //            {
                //                if (u.VendorId != null)
                //                {
                //                    if (u.VendorId.ToString() == vendor)
                //                    {
                //                        li = new SelectListItem();
                //                        li.Text = u.FullName;
                //                        li.Value = u.Id.ToString();
                //                        list.Add(li);
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                if (u != null)
                //                {
                //                    li = new SelectListItem();
                //                    li.Text = u.FullName;
                //                    li.Value = u.Id.ToString();
                //                    list.Add(li);
                //                }
                //            }
                //        });
                //    break;
                case "工務主管":
                    s = roleManager.GetUsersInRole("RepMgr").ToList();
                    list = new List<SelectListItem>();
                    foreach (string l in s)
                    {
                        u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                        if (!string.IsNullOrEmpty(u.DptId))
                        {
                            li = new SelectListItem();
                            li.Text = u.FullName + "(" + u.UserName + ")";
                            li.Value = u.Id.ToString();
                            list.Add(li);
                        }
                    }
                    break;
                case "工務主任":
                    s = roleManager.GetUsersInRole("RepDirector").ToList();
                    list = new List<SelectListItem>();
                    foreach (string l in s)
                    {
                        u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                        if (!string.IsNullOrEmpty(u.DptId))
                        {
                            li = new SelectListItem();
                            li.Text = u.FullName + "(" + u.UserName + ")";
                            li.Value = u.Id.ToString();
                            list.Add(li);
                        }
                    }
                    break;
                case "工務經辦":
                    list = new List<SelectListItem>();
                    u = _context.AppUsers.Where(ur => ur.UserName == "1814").FirstOrDefault();
                    if (!string.IsNullOrEmpty(u.DptId))
                    {
                        li = new SelectListItem();
                        li.Text = u.FullName + "(" + u.UserName + ")";
                        li.Value = u.Id.ToString();
                        list.Add(li);
                    }
                    break;
                case "營建主管":
                    s = roleManager.GetUsersInRole("CaPMgr").ToList();
                    list = new List<SelectListItem>();
                    foreach (string l in s)
                    {
                        u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                        if (!string.IsNullOrEmpty(u.DptId))
                        {
                            li = new SelectListItem();
                            li.Text = u.FullName + "(" + u.UserName + ")";
                            li.Value = u.Id.ToString();
                            list.Add(li);
                        }
                    }
                    break;
                case "營建主任":
                    s = roleManager.GetUsersInRole("CaPDirector").ToList();
                    list = new List<SelectListItem>();
                    foreach (string l in s)
                    {
                        u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                        if (!string.IsNullOrEmpty(u.DptId))
                        {
                            li = new SelectListItem();
                            li.Text = u.FullName + "(" + u.UserName + ")";
                            li.Value = u.Id.ToString();
                            list.Add(li);
                        }
                    }
                    break;
                case "單位主管":
                case "單位主任":
                    //s = roleManager.GetUsersInRole("Manager").ToList();
                    /* 擷取申請人單位底下所有人員 */
                    //string c = _context.AppUsers.Find(r.UserId).DptId;
                    //var dptUsers = _context.AppUsers.Where(a => a.DptId == c).ToList();
                    //list = new List<SelectListItem>();
                    //foreach (var item in dptUsers)
                    //{
                    //        li = new SelectListItem();
                    //        li.Text = item.FullName;
                    //        li.Value = item.Id.ToString();
                    //        list.Add(li);
                    //}
                    break;
                case "單位副院長":
                    s = roleManager.GetUsersInRole("ViceSI").ToList();
                    list = new List<SelectListItem>();
                    foreach (string l in s)
                    {
                        u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                        if (!string.IsNullOrEmpty(u.DptId))
                        {
                            li = new SelectListItem();
                            li.Text = u.FullName + "(" + u.UserName + ")";
                            li.Value = u.Id.ToString();
                            list.Add(li);
                        }
                    }
                    break;
                case "申請人":
                    if (r != null)
                    {
                        list = new List<SelectListItem>();
                        li = new SelectListItem();
                        li.Text = r.UserName;
                        li.Value = r.UserId.ToString();
                        list.Add(li);
                    }
                    else
                    {
                        list = new List<SelectListItem>();
                        li = new SelectListItem();
                        li.Text = "宋大衛";
                        li.Value = "000";
                        list.Add(li);
                    }
                    break;
                case "驗收人":
                    if (_context.RepairEmps.Where(emp => emp.DocId == docid).Count() <= 0)
                    {
                        throw new Exception("沒有維修工程師紀錄!!");
                    }
                    if (r != null)
                    {
                        /* 與驗收人同單位的成員(包括驗收人) */
                        var checkerDptId = _context.AppUsers.Find(r.CheckerId).DptId;
                        List<AppUserModel> ul = _context.AppUsers.Where(f => f.DptId == checkerDptId)
                                                                 .Where(f => f.Status == "Y").ToList();
                        if (asset != null)
                        {
                            if(asset.DelivDpt != r.DptId)
                            {
                                ul.AddRange(_context.AppUsers.Where(f => f.DptId == asset.DelivDpt)
                                                             .Where(f => f.Status == "Y").ToList());
                            }
                        }
                        /* 驗收人 */
                        var checker = _context.AppUsers.Find(r.CheckerId);
                        list = new List<SelectListItem>();
                        li = new SelectListItem();
                        li.Text = checker.FullName + "(" + checker.UserName + ")";
                        li.Value = checker.Id.ToString();
                        list.Add(li);

                        foreach (AppUserModel l in ul)
                        {
                            /* 申請人以外的成員 */
                            if(l.Id != r.UserId)
                            {
                                li = new SelectListItem();
                                li.Text = l.FullName + "(" + l.UserName + ")";
                                li.Value = l.Id.ToString();
                                list.Add(li);
                            }
                        }
                    }
                    break;
                case "工務/營建工程師":

                    /* Get all engineers. */
                    s = roleManager.GetUsersInRole("RepEngineer").ToList();
                    /* Get default engineer. */
                    var repEngId = _context.AppUsers.Find(r.EngId).UserName;
                    var engTemp = _context.AppUsers.Find(r.EngId);
                    var lastFlowEng = _context.RepairFlows.Where(rf => rf.DocId == docid)
                                                          .Where(rf => rf.Cls.Contains("工程師"))
                                                          .OrderByDescending(rf => rf.StepId).FirstOrDefault();
                    if (lastFlowEng != null)
                    {
                        repEngId = _context.AppUsers.Find(lastFlowEng.UserId).UserName;
                        engTemp = _context.AppUsers.Find(lastFlowEng.UserId);
                    }

                    list = new List<SelectListItem>();
                    /* 負責工程師 */
                    li = new SelectListItem();
                    li.Text = engTemp.FullName + "(" + engTemp.UserName + ")";
                    li.Value = engTemp.Id.ToString();
                    list.Add(li);
                    /* 其他工程師 */
                    foreach (string l in s)
                    {
                        u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                        if (u != null && l != repEngId)
                        {
                            li = new SelectListItem();
                            li.Text = u.FullName + "(" + u.UserName + ")";
                            li.Value = u.Id.ToString();
                            list.Add(li);
                        }
                    }
                    break;
                case "列管財產負責人":
                    list = new List<SelectListItem>();
                    //u = _context.AppUsers.Where(ur => ur.UserName == "181151").FirstOrDefault();
                    u = _context.AppUsers.Where(ur => ur.UserName == "53929").FirstOrDefault();
                    if (!string.IsNullOrEmpty(u.DptId))
                    {
                        li = new SelectListItem();
                        li.Text = u.FullName + "(" + u.UserName + ")";
                        li.Value = u.Id.ToString();
                        list.Add(li);
                    }
                    break;
                case "固資財產負責人":
                    list = new List<SelectListItem>();
                    u = _context.AppUsers.Where(ur => ur.UserName == "1814").FirstOrDefault();
                    if (!string.IsNullOrEmpty(u.DptId))
                    {
                        li = new SelectListItem();
                        li.Text = u.FullName + "(" + u.UserName + ")";
                        li.Value = u.Id.ToString();
                        list.Add(li);
                    }
                    break;
                default:
                    list = new List<SelectListItem>();
                    break;
            }
            return Json(list);
        }

        [HttpPost]
        public JsonResult CheckDealStatus(string docId)
        {
            bool checkResult = false;
            var repairDtl = _context.RepairDtls.Find(docId);
            var repairFlow = _context.RepairFlows.Where(rf => rf.DocId == docId)
                                                 .OrderByDescending(o => o.StepId).FirstOrDefault();

            if (repairFlow.Cls.Contains("工程師") && repairDtl.DealState == 1)
            {
                checkResult = true;
            }
            return Json(checkResult);
        }
    }
}