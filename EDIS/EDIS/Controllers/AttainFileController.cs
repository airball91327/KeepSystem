using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDIS.Data;
using EDIS.Models.RepairModels;
using System.IO;
using EDIS.Repositories;
using EDIS.Models.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace EDIS.Controllers
{
    [Authorize]
    public class AttainFileController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IRepository<AppUserModel, int> _userRepo;
        private readonly IHostingEnvironment _hostingEnvironment;

        public AttainFileController(ApplicationDbContext context,
                                    IRepository<AppUserModel, int> userRepo,
                                    IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _userRepo = userRepo;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: AttainFile
        public async Task<IActionResult> Index()
        {
            return View(await _context.AttainFiles.ToListAsync());
        }

        [HttpPost]
        public ActionResult List(string docid = null, string doctyp = null)
        {
            return ViewComponent("AttainFileList2", new { id = docid, typ = doctyp});
        }

        [HttpPost]
        public ActionResult List3(string docid = null, string doctyp = null)
        {
            return ViewComponent("AttainFileList3", new { id = docid, typ = doctyp });
        }

        [HttpPost]
        public async Task<IActionResult> Upload2(AttainFileModel attainFile)
        {
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            long size = attainFile.Files.Sum(f => f.Length);

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            if (ModelState.IsValid)
            {
                try
                {

                    string s = "/Files";
//#if DEBUG
//                    s = "/App_Data";
//#endif
                    switch (attainFile.DocType)
                    {
                        case "0":
                            s += "/Budget/";
                            break;
                        case "1":
                            s += "/Repair/";
                            break;
                        case "2":
                            s += "/Keep/";
                            break;
                        case "3":
                            s += "/BuyEvaluate/";
                            break;
                        case "4":
                            s += "/Delivery/";
                            break;
                        case "5":
                            s += "/Asset/";
                            break;
                    }
                    var i = _context.AttainFiles
                                    .Where(a => a.DocType == attainFile.DocType)
                                    .Where(a => a.DocId == attainFile.DocId).ToList();
                    attainFile.SeqNo = i.Count == 0 ? 1 : i.Select(a => a.SeqNo).Max() + 1;

                    string WebRootPath = _hostingEnvironment.WebRootPath;
                    string path = Path.Combine(WebRootPath + s + attainFile.DocId + "_"
                    + attainFile.SeqNo.ToString() + Path.GetExtension(attainFile.Files[0].FileName));
                    // Upload files.
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await attainFile.Files[0].CopyToAsync(stream);
                    }

                    // Save file details to AttainFiles table.
                    string filelink = attainFile.DocId + "_"
                    + attainFile.SeqNo.ToString() + Path.GetExtension(attainFile.Files[0].FileName);
                    switch (attainFile.DocType)
                    {
                        case "0":
                            attainFile.FileLink = "Budget/" + filelink;
                            break;
                        case "1":
                            attainFile.FileLink = "Repair/" + filelink;
                            break;
                        case "2":
                            attainFile.FileLink = "Keep/" + filelink;
                            break;
                        case "3":
                            attainFile.FileLink = "BuyEvaluate/" + filelink;
                            break;
                        case "4":
                            attainFile.FileLink = "Delivery/" + filelink;
                            break;
                        case "5":
                            attainFile.FileLink = "Asset/" + filelink;
                            break;
                    }
                    attainFile.Rtt = DateTime.Now;
                    attainFile.Rtp = ur.Id;
                    _context.AttainFiles.Add(attainFile);
                    _context.SaveChanges();

                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
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

            TempData["SendMsg"] = "上傳成功";
            return RedirectToAction("Edit", "Repair", new { area = "", id = attainFile.DocId });

            //return new JsonResult(attainFile)
            //{
            //    Value = new { success = true, error = "" },
            //};
        }

        [HttpPost]
        public async Task<IActionResult> Upload3(AttainFileModel attainFile)
        {
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            long size = attainFile.Files.Sum(f => f.Length);

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            if (ModelState.IsValid)
            {
                try
                {

                    string s = "/Files";
                    //#if DEBUG
                    //                    s = "/App_Data";
                    //#endif
                    switch (attainFile.DocType)
                    {
                        case "0":
                            s += "/Budget/";
                            break;
                        case "1":
                            s += "/Repair/";
                            break;
                        case "2":
                            s += "/Keep/";
                            break;
                        case "3":
                            s += "/BuyEvaluate/";
                            break;
                        case "4":
                            s += "/Delivery/";
                            break;
                        case "5":
                            s += "/Asset/";
                            break;
                    }
                    var i = _context.AttainFiles
                                    .Where(a => a.DocType == attainFile.DocType)
                                    .Where(a => a.DocId == attainFile.DocId).ToList();
                    attainFile.SeqNo = i.Count == 0 ? 1 : i.Select(a => a.SeqNo).Max() + 1;

                    string WebRootPath = _hostingEnvironment.WebRootPath;
                    string path = Path.Combine(WebRootPath + s + attainFile.DocId + "_"
                    + attainFile.SeqNo.ToString() + Path.GetExtension(attainFile.Files[0].FileName));
                    // Upload files.
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await attainFile.Files[0].CopyToAsync(stream);
                    }

                    // Save file details to AttainFiles table.
                    string filelink = attainFile.DocId + "_"
                    + attainFile.SeqNo.ToString() + Path.GetExtension(attainFile.Files[0].FileName);
                    switch (attainFile.DocType)
                    {
                        case "0":
                            attainFile.FileLink = "Budget/" + filelink;
                            break;
                        case "1":
                            attainFile.FileLink = "Repair/" + filelink;
                            break;
                        case "2":
                            attainFile.FileLink = "Keep/" + filelink;
                            break;
                        case "3":
                            attainFile.FileLink = "BuyEvaluate/" + filelink;
                            break;
                        case "4":
                            attainFile.FileLink = "Delivery/" + filelink;
                            break;
                        case "5":
                            attainFile.FileLink = "Asset/" + filelink;
                            break;
                    }
                    attainFile.Rtt = DateTime.Now;
                    attainFile.Rtp = ur.Id;
                    _context.AttainFiles.Add(attainFile);
                    _context.SaveChanges();

                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
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

            return new JsonResult(attainFile)
            {
                Value = new { success = true, error = "" },
            };
        }

        public ActionResult Upload(string doctype, string docid)
        {
            AttainFileModel attainFile = new AttainFileModel();
            attainFile.DocType = doctype;
            attainFile.DocId = docid;
            attainFile.SeqNo = 1;
            attainFile.IsPublic = "N";
            attainFile.FileLink = "default";

            return View(attainFile);
        }
        [HttpPost]
        public async Task<IActionResult> Upload(AttainFileModel attainFile)
        {
            var ur = _userRepo.Find(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            long size = attainFile.Files.Sum(f => f.Length);

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            if (ModelState.IsValid)
            {
                try
                {

                    string s = "/Files";
                    //#if DEBUG
                    //                    s = "/App_Data";
                    //#endif
                    switch (attainFile.DocType)
                    {
                        case "0":
                            s += "/Budget/";
                            break;
                        case "1":
                            s += "/Repair/";
                            break;
                        case "2":
                            s += "/Keep/";
                            break;
                        case "3":
                            s += "/BuyEvaluate/";
                            break;
                        case "4":
                            s += "/Delivery/";
                            break;
                        case "5":
                            s += "/Asset/";
                            break;
                    }
                    var i = _context.AttainFiles
                                    .Where(a => a.DocType == attainFile.DocType)
                                    .Where(a => a.DocId == attainFile.DocId).ToList();
                    attainFile.SeqNo = i.Count == 0 ? 1 : i.Select(a => a.SeqNo).Max() + 1;

                    string WebRootPath = _hostingEnvironment.WebRootPath;
                    string path = Path.Combine(WebRootPath + s + attainFile.DocId + "_"
                    + attainFile.SeqNo.ToString() + Path.GetExtension(attainFile.Files[0].FileName));
                    // Upload files.
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await attainFile.Files[0].CopyToAsync(stream);
                    }

                    // Save file details to AttainFiles table.
                    string filelink = attainFile.DocId + "_"
                    + attainFile.SeqNo.ToString() + Path.GetExtension(attainFile.Files[0].FileName);
                    switch (attainFile.DocType)
                    {
                        case "0":
                            attainFile.FileLink = "Budget/" + filelink;
                            break;
                        case "1":
                            attainFile.FileLink = "Repair/" + filelink;
                            break;
                        case "2":
                            attainFile.FileLink = "Keep/" + filelink;
                            break;
                        case "3":
                            attainFile.FileLink = "BuyEvaluate/" + filelink;
                            break;
                        case "4":
                            attainFile.FileLink = "Delivery/" + filelink;
                            break;
                        case "5":
                            attainFile.FileLink = "Asset/" + filelink;
                            break;
                    }
                    attainFile.Rtt = DateTime.Now;
                    attainFile.Rtp = ur.Id;
                    _context.AttainFiles.Add(attainFile);
                    _context.SaveChanges();

                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
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

            return Ok();

            //return new JsonResult(attainFile)
            //{
            //    Value = new { success = true, error = "" },
            //};
        }

        // GET: AttainFile/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attainFileModel = await _context.AttainFiles.SingleOrDefaultAsync(m => m.DocType == id);
            if (attainFileModel == null)
            {
                return NotFound();
            }
            return View(attainFileModel);
        }

        public ActionResult Delete2(string id = null, int seq = 0, string typ = null)
        {
            string WebRootPath = _hostingEnvironment.WebRootPath;
            string path1 = Path.Combine(WebRootPath + "/Files/");
            AttainFileModel attainfiles = _context.AttainFiles.Find(typ, id, seq);
            if (attainfiles != null)
            {
                FileInfo ff;
                try
                {
                    if (typ == "2")
                    {
                        ff = new FileInfo(Path.Combine(path1, attainfiles.FileLink.Replace("Files/", "")));
                        ff.Delete();
                    }
                    else
                    {
                        ff = new FileInfo(Path.Combine(path1, attainfiles.FileLink));
                        ff.Delete();
                    }
                }
                catch (Exception e)
                {
                    return Content(e.Message);
                }
                _context.AttainFiles.Remove(attainfiles);
                _context.SaveChanges();
            }
            List<AttainFileModel> af = _context.AttainFiles.Where(f => f.DocId == id)
                                                           .Where(f => f.DocType == typ).ToList();

            return ViewComponent("AttainFileList2", new { id = id, typ = typ });
        }

        /* For Create View's scale.*/
        public ActionResult Delete3(string id = null, int seq = 0, string typ = null)
        {
            string WebRootPath = _hostingEnvironment.WebRootPath;
            string path1 = Path.Combine(WebRootPath + "/Files/");
            AttainFileModel attainfiles = _context.AttainFiles.Find(typ, id, seq);
            if (attainfiles != null)
            {
                FileInfo ff;
                try
                {
                    if (typ == "2")
                    {
                        ff = new FileInfo(Path.Combine(path1, attainfiles.FileLink.Replace("Files/", "")));
                        ff.Delete();
                    }
                    else
                    {
                        ff = new FileInfo(Path.Combine(path1, attainfiles.FileLink));
                        ff.Delete();
                    }
                }
                catch (Exception e)
                {
                    return Content(e.Message);
                }
                _context.AttainFiles.Remove(attainfiles);
                _context.SaveChanges();
            }
            List<AttainFileModel> af = _context.AttainFiles.Where(f => f.DocId == id)
                                                           .Where(f => f.DocType == typ).ToList();

            return ViewComponent("AttainFileList3", new { id = id, typ = typ });
        }

        private bool AttainFileModelExists(string id)
        {
            return _context.AttainFiles.Any(e => e.DocType == id);
        }
    }
}
