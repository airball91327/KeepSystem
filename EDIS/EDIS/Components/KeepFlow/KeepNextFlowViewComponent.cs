using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.KeepModels;
using EDIS.Models.RepairModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.KeepFlow
{
    public class KeepNextFlowViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public KeepNextFlowViewComponent(ApplicationDbContext context,
                                         IRepository<AppUserModel, int> userRepo,
                                         CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            /* Get keep and flow details. */
            KeepModel keep = _context.Keeps.Find(id);
            KeepFlowModel keepFlow = _context.KeepFlows.Where(f => f.DocId == id && f.Status == "?")
                                                           .FirstOrDefault();

            /* Insert values. */
            AssignModel assign = new AssignModel();
            assign.DocId = id;

            /* 根據當下流程的人員做額外的流程控管 */
            List<SelectListItem> listItem = new List<SelectListItem>();
            if (keepFlow != null)
            {
                assign.Cls = keepFlow.Cls;
                //if (repairFlow.Cls == "申請人")    //統一回申請人結案
                //{   /* 廢除選項在首頁 */
                //    listItem.Add(new SelectListItem { Text = "廢除", Value = "廢除" });
                //}
                if (keepFlow.Cls == "驗收人")    //統一回申請人=驗收人結案
                {
                    listItem.Add(new SelectListItem { Text = "結案", Value = "結案" });
                }
                if (keepFlow.Cls == "工務/營建工程師")   //工務/營建工程師自己為驗收人時
                {
                    if (keep.CheckerId == keepFlow.UserId)  //驗收人為自己
                    {
                        listItem.Add(new SelectListItem { Text = "結案", Value = "結案" });
                    }
                }
            }

            listItem.Add(new SelectListItem { Text = "申請人", Value = "申請人" });
            listItem.Add(new SelectListItem { Text = "驗收人", Value = "驗收人" });
            listItem.Add(new SelectListItem { Text = "單位主管", Value = "單位主管" });
            listItem.Add(new SelectListItem { Text = "單位主任", Value = "單位主任" });
            listItem.Add(new SelectListItem { Text = "單位直屬院長室主管", Value = "單位直屬院長室主管" });
            listItem.Add(new SelectListItem { Text = "工務/營建工程師", Value = "工務/營建工程師" });
            listItem.Add(new SelectListItem { Text = "工務主管", Value = "工務主管" });
            listItem.Add(new SelectListItem { Text = "工務主任", Value = "工務主任" });
            listItem.Add(new SelectListItem { Text = "營建主管", Value = "營建主管" });
            listItem.Add(new SelectListItem { Text = "營建主任", Value = "營建主任" });
            listItem.Add(new SelectListItem { Text = "列管財產負責人", Value = "列管財產負責人" });
            listItem.Add(new SelectListItem { Text = "固資財產負責人", Value = "固資財產負責人" });
            listItem.Add(new SelectListItem { Text = "其他", Value = "其他" });

            ViewData["FlowCls"] = new SelectList(listItem, "Value", "Text", "");

            List<SelectListItem> listItem3 = new List<SelectListItem>();
            listItem3.Add(new SelectListItem { Text = "", Value = "" });
            ViewData["FlowUid"] = new SelectList(listItem3, "Value", "Text", "");

            assign.Hint = "";

            return View(assign);
        }

    }
}
