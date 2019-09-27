using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
using Microsoft.EntityFrameworkCore;

namespace EDIS.Controllers.WebApi
{
    [Route("WebApi/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class RepCreateController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<RepairModel, string> _repRepo;
        private readonly IRepository<RepairDtlModel, string> _repdtlRepo;
        private readonly IRepository<RepairFlowModel, string[]> _repflowRepo;
        private readonly IRepository<DepartmentModel, string> _dptRepo;
        private readonly IRepository<DocIdStore, string[]> _dsRepo;
        private readonly IRepository<BuildingModel, int> _buildRepo;
        private readonly IEmailSender _emailSender;

        public RepCreateController(ApplicationDbContext context,
                                   IRepository<RepairModel, string> repairRepo,
                                   IRepository<RepairDtlModel, string> repairdtlRepo,
                                   IRepository<RepairFlowModel, string[]> repairflowRepo,
                                   IRepository<DepartmentModel, string> dptRepo,
                                   IRepository<DocIdStore, string[]> dsRepo,
                                   IRepository<BuildingModel, int> buildRepo,
                                   IEmailSender emailSender)
        {
            _context = context;
            _repRepo = repairRepo;
            _repdtlRepo = repairdtlRepo;
            _repflowRepo = repairflowRepo;
            _dptRepo = dptRepo;
            _dsRepo = dsRepo;
            _buildRepo = buildRepo;
            _emailSender = emailSender;
        }

        public class Root
        {
            [Required]
            public string UsrID { get; set; }
            /// <summary>
            /// Hash MD5 加密
            /// </summary>
            [Required]
            public string Passwd { get; set; }  //Hash MD5 加密
            /// <summary>
            /// 結果代碼  0: 成功
            /// </summary>
            public string Code { get; set; }    //結果代碼  0: 成功
            public string Msg { get; set; }     //結果訊息
            [Required]
            public string SerNo { get; set; }   //事件處理編號
            public string Mno { get; set; }     //維修單號
            [Required]
            public string Building { get; set; }//大樓
            [Required]
            public string Floor { get; set; }   //樓層
            [Required]
            public string Point { get; set; }   //故障點編號
            [Required]
            public string Name { get; set; }    //故障點名稱
            [Required]
            public string Area { get; set; }    //區域
            [Required]
            public string Des { get; set; }     //處理描述
        }

        //// GET: WebApi/RepCreate
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET: WebApi/RepCreate/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: WebApi/RepCreate
        /// <summary>
        /// 工務請修系統提供之WebApi，驗證並新增工務請修單，回傳單號。
        /// </summary>
        /// <param name="root">客戶指定傳入之XML格式參數</param>
        [HttpPost]
        public IActionResult Post([FromBody] Root root)
        {
            var userName = root.UsrID;
            var buildingId = root.Building;
            var floorId = root.Floor;
            var areaId = root.Area;
            AppUserModel ur = _context.AppUsers.Where(u => u.UserName == userName).FirstOrDefault();

            if (ur != null)   //Check UserName
            {
                //密碼不在此比對
                string userMD5HashPW = GetMd5Hash(ur.Password); //Get hashed result to compare.

                if (root.Passwd == userMD5HashPW)   //CheckPassWord
                {
                    RepairModel repair = new RepairModel();
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
                    repair.LocType = "本單位";
                    repair.RepType = "請修";
                    repair.Ext = ur.Ext == null ? "" : ur.Ext;
                    repair.TroubleDes = "【事件處理編號:" + root.SerNo + "】" + "\n" + root.Des;
                    repair.AssetNo = root.Point;
                    repair.AssetName = root.Name;
                    repair.Building = buildingId;
                    repair.Floor = floorId;
                    repair.Area = areaId;

                    var engId = GetAreaEngId(Convert.ToInt32(buildingId), floorId, areaId);
                    repair.EngId = engId;
                    repair.CheckerId = ur.Id;

                    /* 如有代理人，將工程師改為代理人*/
                    var subStaff = _context.EngSubStaff.SingleOrDefault(e => e.EngId == repair.EngId);
                    if (subStaff != null)
                    {
                        int startDate = Convert.ToInt32(subStaff.StartDate.ToString("yyyyMMdd"));
                        int endDate = Convert.ToInt32(subStaff.EndDate.ToString("yyyyMMdd"));
                        int today = Convert.ToInt32(DateTime.UtcNow.AddHours(08).ToString("yyyyMMdd"));
                        /* 如在代理期間內，將代理人指定為負責工程師 */
                        if (today >= startDate && today <= endDate)
                        {
                            repair.EngId = subStaff.SubstituteId;
                        }
                    }

                    string msg = "";
                    try
                    {
                        // Create Repair Doc.
                        repair.ApplyDate = DateTime.Now;
                        _repRepo.Create(repair);

                        // Create Repair Details.
                        RepairDtlModel dtl = new RepairDtlModel();
                        dtl.DocId = repair.DocId;
                        dtl.DealState = 1;  // 處理狀態"未處理"
                        _repdtlRepo.Create(dtl);

                        //Create first Repair Flow.
                        RepairFlowModel flow = new RepairFlowModel();
                        flow.DocId = repair.DocId;
                        flow.StepId = 1;
                        flow.UserId = ur.Id;
                        flow.Status = "1";  // 流程狀態"已處理"
                        flow.Rtp = ur.Id;
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
                        flow.Cls = "工務/營建工程師";
                        // If repair type is "增設", send next flow to department manager.
                        if (repair.RepType == "增設")
                        {
                            flow.UserId = Convert.ToInt32(repair.DptMgrId);
                            flow.Cls = "單位主管";
                        }
                        _repflowRepo.Create(flow);

                        repair.BuildingName = _context.Buildings.Where(b => b.BuildingId == Convert.ToInt32(repair.Building)).FirstOrDefault().BuildingName;
                        repair.FloorName = _context.Floors.Where(f => f.BuildingId == Convert.ToInt32(repair.Building) && f.FloorId == repair.Floor).FirstOrDefault().FloorName;
                        repair.AreaName = _context.Places.Where(p => p.BuildingId == Convert.ToInt32(repair.Building) && p.FloorId == repair.Floor && p.PlaceId == repair.Area).FirstOrDefault().PlaceName;
                        //Send Mail 
                        //To user and the next flow user.
                        Tmail mail = new Tmail();
                        string body = "";
                        var mailToUser = ur;
                        mail.from = new System.Net.Mail.MailAddress(mailToUser.Email); //u.Email
                        mailToUser = _context.AppUsers.Find(flow.UserId);
                        mail.to = new System.Net.Mail.MailAddress(mailToUser.Email); //u.Email
                                                                                     //mail.cc = new System.Net.Mail.MailAddress("344027@cch.org.tw");
                        mail.message.Subject = "工務智能請修系統[請修案]：設備名稱： " + repair.AssetName;
                        body += "<p>表單編號：" + repair.DocId + "</p>";
                        body += "<p>申請日期：" + repair.ApplyDate.ToString("yyyy/MM/dd") + "</p>";
                        body += "<p>申請人：" + repair.UserName + "</p>";
                        body += "<p>財產編號：" + repair.AssetNo + "</p>";
                        body += "<p>設備名稱：" + repair.AssetName + "</p>";
                        body += "<p>故障描述：" + repair.TroubleDes + "</p>";
                        body += "<p>請修地點：" + repair.PlaceLoc + " " + repair.BuildingName + " " + repair.FloorName + " " + repair.AreaName + "</p>";
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
                        //mail.SendMail();

                        Root successXML = new Root { Code = "0", Msg = "Success", SerNo = root.SerNo, Mno = repair.DocId };
                        return Ok(successXML);
                    }
                    catch (Exception ex)
                    {
                        msg = ex.Message;
                    }
                    Root errorXMLMsg = new Root { Code = "400", Msg = msg, SerNo = root.SerNo, Mno = "" };
                    return BadRequest(errorXMLMsg);
                }
                else
                {
                    Root errorXMLMsg = new Root { Code = "400", Msg = "PassWord is incorrect.", SerNo = root.SerNo, Mno = "" };
                    return BadRequest(errorXMLMsg);
                }
            }
            else
            {
                Root errorXMLMsg = new Root { Code = "400", Msg = "UserID is not exist.", SerNo = root.SerNo, Mno = "" };
                return BadRequest(errorXMLMsg);
            }
        }

        //// PUT: WebApi/RepCreate/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE: WebApi/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}

        public static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        public string GetID2()
        {
            string did = "";
            try
            {
                DocIdStore ds = new DocIdStore();
                ds.DocType = "請修";
                string s = _dsRepo.Find(x => x.DocType == "請修").Select(x => x.DocId).Max();
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
                    var dataIsExist = _dsRepo.Find(x => x.DocType == "請修");
                    if (dataIsExist.Count() == 0)
                    {
                        _dsRepo.Create(ds);
                    }
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return did;
        }

        public int GetAreaEngId(int BuildingId, string FloorId, string PlaceId)
        {
            var engineers = _context.EngsInDepts.Include(e => e.AppUsers).Include(e => e.Departments)
                                                .Where(e => e.BuildingId == BuildingId &&
                                                            e.FloorId == FloorId &&
                                                            e.PlaceId == PlaceId).ToList();

            /* 擷取預設負責工程師 */
            if (engineers.Count() == 0)  //該部門無預設工程師
            {
                var engId = _context.AppUsers.Where(a => a.UserName == "181316").FirstOrDefault().Id;
                return engId;
            }
            else
            {
                if (engineers.Count() > 1)
                {
                    var eng = engineers.Join(_context.EngDealingDocs, ed => ed.EngId, e => e.EngId,
                                (ed, e) => new
                                {
                                    ed.EngId,
                                    ed.UserName,
                                    ed.AppUsers.FullName,
                                    e.DealingDocs
                                }).OrderBy(o => o.DealingDocs).FirstOrDefault();
                    return eng.EngId;
                }
                else
                {
                    var eng = engineers.Select(e => new
                    {
                        e.EngId,
                        e.UserName,
                        e.AppUsers.FullName,
                    }).FirstOrDefault();
                    return eng.EngId;
                }
            }
        }
    }
}
