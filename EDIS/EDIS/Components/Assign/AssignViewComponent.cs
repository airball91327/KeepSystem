using EDIS.Models.RepairModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.Assign
{
    public class AssignViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            AssignModel assign = new AssignModel();
            //repair.Buildings = new List<SelectListItem>
            //{
            //    new SelectListItem{Text = "第一醫療大樓",Value="第一醫療大樓"},
            //    new SelectListItem{Text = "第二醫療大樓",Value="第二醫療大樓"},
            //    new SelectListItem{Text = "第三醫療大樓",Value="第三醫療大樓"},
            //    new SelectListItem{Text = "中華路院區",Value="中華路院區"},
            //    new SelectListItem{Text = "兒童醫院",Value="兒童醫院"},
            //    new SelectListItem{Text = "向上大樓",Value="向上大樓"}
            //};

            return View(assign);
        }
    }
}
