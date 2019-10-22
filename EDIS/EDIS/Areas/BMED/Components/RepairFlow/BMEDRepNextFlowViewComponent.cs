using EDIS.Areas.BMED.Data;
using EDIS.Models.Identity;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Areas.BMED.Repositories;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Areas.BMED.Components.RepairFlow
{
    public class BMEDRepNextFlowViewComponent : ViewComponent
    {
        private readonly BMEDDbContext _context;
        private readonly BMEDIRepository<RepairFlowModel, string[]> _repflowRepo;
        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly CustomUserManager userManager;

        public BMEDRepNextFlowViewComponent(BMEDDbContext context,
                                            BMEDIRepository<RepairFlowModel, string[]> repairflowRepo,
                                            IRepository<AppUserModel, int> userRepo,
                                            CustomUserManager customUserManager)
        {
            _context = context;
            _repflowRepo = repairflowRepo;
            _userRepo = userRepo;
            userManager = customUserManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            /* Get repair and flow details. */
            RepairModel repair = _context.BMEDRepairs.Find(id);
            RepairDtlModel repairDtl = _context.BMEDRepairDtls.Find(id);
            RepairFlowModel repairFlow = _context.BMEDRepairFlows.Where(f => f.DocId == id && f.Status == "?")
                                                                 .FirstOrDefault();
            /* 增設流程 */
            List<SelectListItem> listItem = new List<SelectListItem>();
            if (repair.RepType == "增設")
            {
                listItem.Add(new SelectListItem { Text = "申請人", Value = "申請人" });
                listItem.Add(new SelectListItem { Text = "驗收人", Value = "驗收人" });
                listItem.Add(new SelectListItem { Text = "單位主管", Value = "單位主管" });
                listItem.Add(new SelectListItem { Text = "單位主任", Value = "單位主任" });
                listItem.Add(new SelectListItem { Text = "單位副院長", Value = "單位副院長" });
                listItem.Add(new SelectListItem { Text = "維修工程師", Value = "維修工程師" });
                listItem.Add(new SelectListItem { Text = "設備工程師", Value = "設備工程師" });
                listItem.Add(new SelectListItem { Text = "醫工主管", Value = "醫工主管" });
                listItem.Add(new SelectListItem { Text = "賀康主管", Value = "賀康主管" });
                //listItem.Add(new SelectListItem { Text = "醫工主任", Value = "醫工主任" });
            }
            else  //維修流程
            {
                listItem.Add(new SelectListItem { Text = "申請人", Value = "申請人" });
                listItem.Add(new SelectListItem { Text = "驗收人", Value = "驗收人" });
                listItem.Add(new SelectListItem { Text = "單位主管", Value = "單位主管" });
                listItem.Add(new SelectListItem { Text = "維修工程師", Value = "維修工程師" });
                listItem.Add(new SelectListItem { Text = "設備工程師", Value = "設備工程師" });
                listItem.Add(new SelectListItem { Text = "醫工主管", Value = "醫工主管" });
                listItem.Add(new SelectListItem { Text = "賀康主管", Value = "賀康主管" });

                //額外流程控管
                if (repairDtl.IsCharged == "Y" && repairDtl.NotInExceptDevice == "N")   //有費用 & 非統包
                {
                    var itemToRemove = listItem.Single(r => r.Value == "驗收人");
                    listItem.Remove(itemToRemove);    //只醫工主管可結案
                }
                if (repairDtl.DealState == 4)   //報廢
                {
                    var itemToRemove = listItem.Single(r => r.Value == "驗收人");
                    listItem.Remove(itemToRemove);    //只醫工主管可結案
                }
                if (repairDtl.IsCharged == "Y" && repairDtl.NotInExceptDevice == "Y")   //有費用 & 統包
                {
                    var itemToRemove = listItem.Single(r => r.Value == "醫工主管");
                    listItem.Remove(itemToRemove);    //移除醫工主管的選項
                }
            }
            //listItem.Add(new SelectListItem { Text = "列管財產負責人", Value = "列管財產負責人" });
            //listItem.Add(new SelectListItem { Text = "固資財產負責人", Value = "固資財產負責人" });
            listItem.Add(new SelectListItem { Text = "其他", Value = "其他" });
            /* Insert values. */
            AssignModel assign = new AssignModel();
            assign.DocId = id;

            /* 根據當下流程的人員做額外的流程控管 */
            if (repairFlow != null)
            {
                assign.Cls = repairFlow.Cls;

                if (repairFlow.Cls == "驗收人" || repairFlow.Cls == "醫工主管")    //驗收人 or 醫工主管結案
                {
                    listItem.Add(new SelectListItem { Text = "結案", Value = "結案" });
                }
                //if (repairFlow.Cls == "醫工工程師")   //醫工工程師自己為驗收人時
                //{
                //    if (repair.CheckerId == repairFlow.UserId)  //驗收人為自己
                //    {
                //        listItem.Add(new SelectListItem { Text = "結案", Value = "結案" });
                //    }
                //}
            }
            ViewData["FlowCls"] = new SelectList(listItem, "Value", "Text", "");

            List<SelectListItem> listItem3 = new List<SelectListItem>();
            listItem3.Add(new SelectListItem { Text = "", Value = "" });
            ViewData["FlowUid"] = new SelectList(listItem3, "Value", "Text", "");

            /* Get Default Checker for MedEngineers to edit. */
            List<SelectListItem> listItem4 = new List<SelectListItem>();
            var defaultChecker = _context.AppUsers.Find(repair.CheckerId);
            listItem4.Add(new SelectListItem { Text = defaultChecker.FullName, Value = defaultChecker.Id.ToString() });
            ViewData["DefaultChecker"] = new SelectList(listItem4, "Value", "Text", defaultChecker.Id.ToString());

            /* 驗收人員所屬部門搜尋的下拉選單資料 */
            var dptList = new[] { "K", "P", "C" };   //本院部門
            var departments = _context.Departments.Where(d => dptList.Contains(d.Loc)).ToList();
            List<SelectListItem> listItem5 = new List<SelectListItem>();
            foreach (var item in departments)
            {
                listItem5.Add(new SelectListItem
                {
                    Text = item.Name_C + "(" + item.DptId + ")",    //show DptName(DptId)
                    Value = item.DptId
                });
            }
            ViewData["BMEDQRYDPT"] = new SelectList(listItem5, "Value", "Text");

            assign.Hint = "申請者→負責工程師→使用單位→[(若有費用及報廢)醫工部主管]→結案";

            /* 於流程頁面顯示請修類型、及處理狀態*/
            string hintRepType = repair.RepType;
            string hintRepState = "";
            var repDtl = _context.BMEDRepairDtls.Where(dtl => dtl.DocId == id).FirstOrDefault();
            if (repDtl != null)
            {
                hintRepState = _context.BMEDDealStatuses.Find(repDtl.DealState).Title;
            }
            ViewData["HintRepType"] = hintRepType + " / " + hintRepState;

            return View(assign);
        }
    }
}
