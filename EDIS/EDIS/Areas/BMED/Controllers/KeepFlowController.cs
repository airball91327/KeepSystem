using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EDIS.Fliters;

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class KeepFlowController : Controller
    {
        private readonly BMEDDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;

        public KeepFlowController(BMEDDbContext context,
                                  IRepository<AppUserModel, int> userRepo,
                                  CustomUserManager customUserManager,
                                  CustomRoleManager customRoleManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
            roleManager = customRoleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult NextFlow(AssignModel assign)
        {
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();

            /* 工程師的流程控管 */
            //if (assign.Cls == "設備工程師")
            //{
            //    /* 如點選有費用、卻無輸入費用明細 */
            //    var isCharged = _context.BMEDKeepDtls.Where(d => d.DocId == assign.DocId).FirstOrDefault().IsCharged;
            //    if (isCharged == "Y")
            //    {
            //        var CheckRepairCost = _context.BMEDKeepCosts.Where(c => c.DocId == assign.DocId).FirstOrDefault();
            //        if (CheckRepairCost == null)
            //        {
            //            throw new Exception("尚未輸入費用明細!!");
            //        }
            //    }
            //}

            if (assign.FlowCls == "結案" || assign.FlowCls == "廢除")
                assign.FlowUid = ur.Id;
            if (ModelState.IsValid)
            {
                KeepFlowModel kf = _context.BMEDKeepFlows.Where(f => f.DocId == assign.DocId && f.Status == "?").FirstOrDefault();
                if (assign.FlowCls == "驗收人")
                {
                    if (_context.BMEDKeepEmps.Where(emp => emp.DocId == assign.DocId).Count() <= 0)
                    {
                        throw new Exception("沒有維修工程師紀錄!!");
                    }
                    else if (_context.BMEDKeepDtls.Find(assign.DocId).EndDate == null)
                    {
                        throw new Exception("沒有完工日!!");
                    }
                    if (_context.BMEDKeepDtls.Find(assign.DocId).Result == null)
                    {
                        throw new Exception("保養結果不可空白!!");
                    }
                    if (string.IsNullOrEmpty(_context.BMEDKeepDtls.Find(assign.DocId).InOut))
                    {
                        throw new Exception("保養方式不可空白!!");
                    }
                }
                if (assign.FlowCls == "結案")
                {
                    KeepDtlModel kd = _context.BMEDKeepDtls.Find(assign.DocId);
                    kd.CloseDate = DateTime.Now;
                    kf.Opinions = "[" + assign.AssignCls + "]" + Environment.NewLine + assign.AssignOpn;
                    kf.Status = "2";
                    kf.UserId = ur.Id;
                    kf.UserName = _context.AppUsers.Find(ur.Id).FullName;
                    kf.Rtt = DateTime.Now;
                    kf.Rtp = ur.Id;
                    _context.Entry(kf).State = EntityState.Modified;
                    _context.Entry(kd).State = EntityState.Modified;

                    _context.SaveChanges();

                    //Send Mail
                    //To all users in this keep's flow.
                    Tmail mail = new Tmail();
                    string body = "";
                    string sto = "";
                    AppUserModel u;
                    KeepModel keep = _context.BMEDKeeps.Find(assign.DocId);
                    mail.from = new System.Net.Mail.MailAddress(ur.Email); //u.Email
                    _context.BMEDKeepFlows.Where(f => f.DocId == assign.DocId)
                            .ToList()
                            .ForEach(f =>
                            {
                                u = _context.AppUsers.Find(f.UserId);
                                sto += u.Email + ",";
                            });
                    mail.sto = sto.TrimEnd(new char[] { ',' });

                    mail.message.Subject = "醫工工務智能保修系統[醫工保養案-結案通知]：設備名稱： " + keep.AssetName;
                    body += "<p>表單編號：" + keep.DocId + "</p>";
                    body += "<p>送單日期：" + keep.SentDate.Value.ToString("yyyy/MM/dd") + "</p>";
                    body += "<p>申請人：" + keep.UserName + "</p>";
                    body += "<p>財產編號：" + keep.AssetNo + "</p>";
                    body += "<p>設備名稱：" + keep.AssetName + "</p>";
                    body += "<p>放置地點：" + keep.PlaceLoc + "</p>";
                    body += "<p>保養結果：" + kd.Result + "</p>";
                    body += "<p>保養描述：" + kd.Memo + "</p>";
                    body += "<p><a href='http://dms.cch.org.tw/EDIS/Account/Login'" + "?DocId=" + keep.DocId + "&dealType=BMEDKeepViews" + ">檢視案件</a></p>";
                    body += "<br/>";
                    body += "<h3>此封信件為系統通知郵件，請勿回覆。</h3>";
                    body += "<br/>";
                    //body += "<h3 style='color:red'>如有任何疑問請聯絡工務部，分機3033或7033。<h3>";
                    mail.message.Body = body;
                    mail.message.IsBodyHtml = true;
                    mail.SendMail();
                }
                else if (assign.FlowCls == "廢除")
                {
                    kf.Opinions = "[廢除]" + Environment.NewLine + assign.AssignOpn;
                    kf.Status = "3";
                    kf.Rtt = DateTime.Now;
                    kf.Rtp = ur.Id;
                    _context.Entry(kf).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                else
                {
                    //轉送下一關卡
                    kf.Opinions = "[" + assign.AssignCls + "]" + Environment.NewLine + assign.AssignOpn;
                    kf.Status = "1";
                    kf.Rtt = DateTime.Now;
                    kf.Rtp = ur.Id;
                    _context.Entry(kf).State = EntityState.Modified;
                    _context.SaveChanges();
                    //
                    KeepFlowModel flow = new KeepFlowModel();
                    flow.DocId = assign.DocId;
                    flow.StepId = kf.StepId + 1;
                    flow.UserId = assign.FlowUid.Value;
                    flow.UserName = _context.AppUsers.Find(assign.FlowUid.Value).FullName;
                    flow.Status = "?";
                    flow.Cls = assign.FlowCls;
                    flow.Rtt = DateTime.Now;
                    _context.BMEDKeepFlows.Add(flow);
                    _context.SaveChanges();

                    //Send Mail
                    //To user and the next flow user.
                    Tmail mail = new Tmail();
                    string body = "";
                    AppUserModel u;
                    KeepModel keep = _context.BMEDKeeps.Find(assign.DocId);
                    mail.from = new System.Net.Mail.MailAddress(ur.Email); //ur.Email
                    u = _context.AppUsers.Find(flow.UserId);
                    mail.to = new System.Net.Mail.MailAddress(u.Email); //u.Email
                                                                        //mail.cc = new System.Net.Mail.MailAddress("99242@cch.org.tw");
                    mail.message.Subject = "醫工工務智能保修系統[醫工保養案]：設備名稱： " + keep.AssetName;
                    body += "<p>表單編號：" + keep.DocId + "</p>";
                    body += "<p>送單日期：" + keep.SentDate.Value.ToString("yyyy/MM/dd") + "</p>";
                    body += "<p>申請人：" + keep.UserName + "</p>";
                    body += "<p>財產編號：" + keep.AssetNo + "</p>";
                    body += "<p>設備名稱：" + keep.AssetName + "</p>";
                    body += "<p>放置地點：" + keep.PlaceLoc + "</p>";
                    body += "<p><a href='http://dms.cch.org.tw/EDIS/Account/Login'" + "?docId=" + keep.DocId + "&dealType=BMEDKeepEdit" + ">處理案件</a></p>";
                    body += "<br/>";
                    body += "<h3>此封信件為系統通知郵件，請勿回覆。</h3>";
                    body += "<br/>";
                    //body += "<h3 style='color:red'>如有任何疑問請聯絡工務部，分機3033或7033。<h3>";
                    mail.message.Body = body;
                    mail.message.IsBodyHtml = true;
                    mail.SendMail();
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
            KeepModel k = _context.BMEDKeeps.Find(docid);
            AssetModel asset = _context.BMEDAssets.Where(a => a.AssetNo == k.AssetNo).FirstOrDefault();

            switch (cls)
            {
                case "維修工程師":
                    roleManager.GetUsersInRole("Engineer").ToList()
                        .ForEach(x =>
                        {
                            u = _context.AppUsers.Where(ur => ur.UserName == x).FirstOrDefault();
                            if (u != null)
                            {
                                li = new SelectListItem();
                                li.Text = u.FullName;
                                li.Value = u.Id.ToString();
                                list.Add(li);
                            }
                        });
                    break;
                case "醫工主管":
                    s = roleManager.GetUsersInRole("MedMgr").ToList();
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
                case "設備主管":
                    s = roleManager.GetUsersInRole("MedAssetMgr").ToList();
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
                case "醫工主任":
                    s = roleManager.GetUsersInRole("MedDirector").ToList();
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
                case "醫工經辦":
                    //list = new List<SelectListItem>();
                    //u = _context.AppUsers.Where(ur => ur.UserName == "1814").FirstOrDefault();
                    //if (!string.IsNullOrEmpty(u.DptId))
                    //{
                    //    li = new SelectListItem();
                    //    li.Text = u.FullName;
                    //    li.Value = u.Id.ToString();
                    //    list.Add(li);
                    //}
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
                    if (k != null)
                    {
                        list = new List<SelectListItem>();
                        li = new SelectListItem();
                        li.Text = k.UserName;
                        li.Value = k.UserId.ToString();
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
                    if (_context.BMEDKeepEmps.Where(emp => emp.DocId == docid).Count() <= 0)
                    {
                        throw new Exception("沒有維修工程師紀錄!!");
                    }
                    else if (_context.BMEDKeepDtls.Find(docid).EndDate == null)
                    {
                        throw new Exception("沒有完工日!!");

                    }
                    else if (_context.BMEDKeepDtls.Find(docid).Result == null ||
                        _context.BMEDKeepDtls.Find(docid).Result == null)
                    {
                        throw new Exception("沒有保養結果!!");
                    }
                    if (k != null)
                    {
                        /* 與驗收人同單位的成員(包括驗收人) */
                        var checkerDptId = _context.AppUsers.Find(k.CheckerId).DptId;
                        List<AppUserModel> ul = _context.AppUsers.Where(f => f.DptId == checkerDptId)
                                                                 .Where(f => f.Status == "Y").ToList();
                        if (asset != null)
                        {
                            if (asset.DelivDpt != k.DptId)
                            {
                                ul.AddRange(_context.AppUsers.Where(f => f.DptId == asset.DelivDpt)
                                                             .Where(f => f.Status == "Y").ToList());
                            }
                        }
                        /* 驗收人 */
                        var checker = _context.AppUsers.Find(k.CheckerId);
                        list = new List<SelectListItem>();
                        li = new SelectListItem();
                        li.Text = checker.FullName + "(" + checker.UserName + ")";
                        li.Value = checker.Id.ToString();
                        list.Add(li);

                        foreach (AppUserModel l in ul)
                        {
                            /* 申請人以外的成員 */
                            if (l.Id != k.UserId)
                            {
                                li = new SelectListItem();
                                li.Text = l.FullName + "(" + l.UserName + ")";
                                li.Value = l.Id.ToString();
                                list.Add(li);
                            }
                        }
                    }
                    break;
                case "設備工程師":

                    /* Get all engineers. */
                    s = roleManager.GetUsersInRole("MedEngineer").ToList();
                    var keepEngId = _context.BMEDAssetKeeps.Find(k.AssetNo).KeepEngId;
                    var keepEng = _context.AppUsers.Find(keepEngId);

                    list = new List<SelectListItem>();
                    /* 負責工程師 */
                    li = new SelectListItem();
                    li.Text = keepEng.FullName + "(" + keepEng.UserName + ")";
                    li.Value = keepEng.Id.ToString();
                    list.Add(li);
                    /* 其他工程師 */
                    foreach (string l in s)
                    {
                        u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                        if (u != null && l != keepEng.UserName)
                        {
                            li = new SelectListItem();
                            li.Text = u.FullName + "(" + u.UserName + ")";
                            li.Value = u.Id.ToString();
                            list.Add(li);
                        }
                    }
                    break;
                case "列管財產負責人":
                    //list = new List<SelectListItem>();
                    //u = _context.AppUsers.Where(ur => ur.UserName == "181151").FirstOrDefault();
                    //if (!string.IsNullOrEmpty(u.DptId))
                    //{
                    //    li = new SelectListItem();
                    //    li.Text = u.FullName;
                    //    li.Value = u.Id.ToString();
                    //    list.Add(li);
                    //}
                    break;
                case "固資財產負責人":
                    //list = new List<SelectListItem>();
                    //u = _context.AppUsers.Where(ur => ur.UserName == "1814").FirstOrDefault();
                    //if (!string.IsNullOrEmpty(u.DptId))
                    //{
                    //    li = new SelectListItem();
                    //    li.Text = u.FullName;
                    //    li.Value = u.Id.ToString();
                    //    list.Add(li);
                    //}
                    break;
                default:
                    list = new List<SelectListItem>();
                    break;
            }
            return Json(list);
        }


    }
}