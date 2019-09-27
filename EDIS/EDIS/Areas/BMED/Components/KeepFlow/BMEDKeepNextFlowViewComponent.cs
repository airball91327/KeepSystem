using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Areas.BMED.Repositories;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Components.KeepFlow
{
    public class BMEDKeepNextFlowViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public BMEDKeepNextFlowViewComponent(BMEDDbContext context,
                                             IRepository<AppUserModel, int> userRepo,
                                             CustomUserManager customUserManager)
        {
            _context = context;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            /* Get repair and flow details. */
            KeepModel keep = _context.BMEDKeeps.Find(id);
            KeepFlowModel keepFlow = _context.BMEDKeepFlows.Where(f => f.DocId == id && f.Status == "?")
                                                           .FirstOrDefault();

            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "申請人", Value = "申請人" });
            listItem.Add(new SelectListItem { Text = "驗收人", Value = "驗收人" });
            listItem.Add(new SelectListItem { Text = "單位主管", Value = "單位主管" });
            listItem.Add(new SelectListItem { Text = "維修工程師", Value = "維修工程師" });
            listItem.Add(new SelectListItem { Text = "設備工程師", Value = "設備工程師" });
            listItem.Add(new SelectListItem { Text = "醫工主管", Value = "醫工主管" });
            listItem.Add(new SelectListItem { Text = "設備主管", Value = "設備主管" });
            //listItem.Add(new SelectListItem { Text = "醫工主任", Value = "醫工主任" });
            listItem.Add(new SelectListItem { Text = "其他", Value = "其他" });

            /* Insert values. */
            AssignModel assign = new AssignModel();
            assign.DocId = id;

            /* 根據當下流程的人員做額外的流程控管 */
            if (keepFlow != null)
            {
                assign.Cls = keepFlow.Cls;

                if (keepFlow.Cls == "驗收人" || keepFlow.Cls == "醫工主管")    //驗收人 or 醫工主管結案
                {
                    listItem.Add(new SelectListItem { Text = "結案", Value = "結案" });
                }

            }
            ViewData["FlowCls"] = new SelectList(listItem, "Value", "Text", "");

            List<SelectListItem> listItem3 = new List<SelectListItem>();
            listItem3.Add(new SelectListItem { Text = "", Value = "" });
            ViewData["FlowUid"] = new SelectList(listItem3, "Value", "Text", "");

            assign.Hint = "";

            return View(assign);
        }

    }
}
