using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.RepairModels;
using EDIS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Components.AttainFile
{
    public class AttainFileUpload2ViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string doctype, string docid)
        {
            AttainFileModel attainFile = new AttainFileModel();
            attainFile.DocType = doctype;
            attainFile.DocId = docid;
            attainFile.SeqNo = 1;
            attainFile.IsPublic = "N";
            attainFile.FileLink = "default";

            return View(attainFile);
        }
    }
}
