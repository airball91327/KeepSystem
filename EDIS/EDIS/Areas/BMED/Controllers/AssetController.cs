using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models.KeepModels;
using EDIS.Areas.BMED.Models.RepairModels;
using EDIS.Models.Identity;
using EDIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace EDIS.Areas.BMED.Controllers
{
    [Area("BMED")]
    [Authorize]
    public class AssetController : Controller
    {
        private readonly BMEDDbContext _context;
        private readonly CustomUserManager userManager;
        private readonly CustomRoleManager roleManager;
        private int pageSize = 100;

        public AssetController(BMEDDbContext context,
                               CustomRoleManager customRoleManager,
                               CustomUserManager customUserManager)
        {
            _context = context;
            roleManager = customRoleManager;
            userManager = customUserManager;
        }

        // GET: BMED/Asset
        public IActionResult Index()
        {
            List<SelectListItem> listItem2 = new List<SelectListItem>();
            SelectListItem li;
            _context.Departments.ToList()
                .ForEach(d =>
                {
                    li = new SelectListItem();
                    li.Text = d.Name_C;
                    li.Value = d.DptId;
                    listItem2.Add(li);

                });
            ViewData["ACCDPT"] = new SelectList(listItem2, "Value", "Text");
            ViewData["DELIVDPT"] = new SelectList(listItem2, "Value", "Text");
            List<SelectListItem> listItem4 = new List<SelectListItem>();
            _context.BMEDDeviceClassCodes.ToList()
                .ForEach(d =>
                {
                    listItem4.Add(new SelectListItem { Text = d.M_name, Value = d.M_code });
                });
            ViewData["BmedNo"] = new SelectList(listItem4, "Value", "Text", "");

            return View();
        }

        // POST: BMED/Asset
        [HttpPost]
        public IActionResult Index(IFormCollection fm, int page = 1)
        {
            QryAsset qryAsset = new QryAsset();
            qryAsset.AssetName = fm["AssetName"];
            qryAsset.AssetNo = fm["AssetNo"];
            qryAsset.AccDpt = fm["AccDpt"];
            qryAsset.DelivDpt = fm["DelivDpt"];
            qryAsset.Type = fm["Type"];
            qryAsset.BmedNo = fm["BmedNo"];
            List<AssetModel> at = new List<AssetModel>();
            List<AssetModel> at2 = new List<AssetModel>();
            _context.BMEDAssets.GroupJoin(_context.Departments, a => a.DelivDpt, d => d.DptId,
                (a, d) => new { Asset = a, Department = d })
                .SelectMany(p => p.Department.DefaultIfEmpty(),
                (x, y) => new { Asset = x.Asset, Department = y })
                .ToList()
                .GroupJoin(_context.AppUsers, e => e.Asset.DelivUid, u => u.Id,
                (e, u) => new { Asset = e, AppUser = u })
                .SelectMany(p => p.AppUser.DefaultIfEmpty(),
                (e, y) => new { Asset = e.Asset.Asset, Department = e.Asset.Department, AppUser = y })
                .ToList()
                .ForEach(p =>
                {
                    p.Asset.DelivDptName = p.Department == null ? "" : p.Department.Name_C;
                    p.Asset.DelivEmp = p.AppUser == null ? "" : p.AppUser.FullName;
                    at.Add(p.Asset);
                });
            at.GroupJoin(_context.Departments, a => a.AccDpt, d => d.DptId,
                (a, d) => new { Asset = a, Department = d })
                .SelectMany(p => p.Department.DefaultIfEmpty(),
                (x, y) => new { Asset = x.Asset, Department = y })
                .ToList()
                .ForEach(p =>
                {
                    p.Asset.AccDptName = p.Department == null ? "" : p.Department.Name_C;
                    at2.Add(p.Asset);
                });
            if (!string.IsNullOrEmpty(qryAsset.AssetNo))
            {
                at2 = at2.Where(a => a.AssetNo == qryAsset.AssetNo).ToList();
            }
            if (!string.IsNullOrEmpty(qryAsset.AssetName))
            {
                at2 = at2.Where(a => a.Cname.Contains(qryAsset.AssetName)).ToList();
            }
            if (!string.IsNullOrEmpty(qryAsset.AccDpt))
            {
                at2 = at2.Where(a => a.AccDpt == qryAsset.AccDpt).ToList();
            }
            if (!string.IsNullOrEmpty(qryAsset.DelivDpt))
            {
                at2 = at2.Where(a => a.DelivDpt == qryAsset.DelivDpt).ToList();
            }
            if (!string.IsNullOrEmpty(qryAsset.BmedNo))
            {
                at2 = at2.Where(a => a.BmedNo != null)
                    .Where(a => a.BmedNo.Contains(qryAsset.BmedNo)).ToList();
            }
            if (!string.IsNullOrEmpty(qryAsset.Type))
            {
                at2 = at2.Where(a => a.Type == qryAsset.Type).ToList();
            }
            // Get MedEngineers to set dropdownlist.
            List<SelectListItem> listItem = new List<SelectListItem>();
            var s = roleManager.GetUsersInRole("MedEngineer").ToList();
            foreach (string l in s)
            {
                AppUserModel u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                if (u != null)
                {
                    listItem.Add(new SelectListItem { Text = u.FullName, Value = u.Id.ToString() });
                }
            }
            ViewData["KeepEngId"] = new SelectList(listItem, "Value", "Text", "");
            ViewData["AssetEngId"] = new SelectList(listItem, "Value", "Text", "");
            //
            if (at2.ToPagedList(page, pageSize).Count <= 0)
                return PartialView("List", at2.ToPagedList(1, pageSize));
            return PartialView("List", at2.ToPagedList(page, pageSize));
        }

        // GET: BMED/Asset/List
        public IActionResult List()
        {
            List<AssetModel> at = _context.BMEDAssets.ToList();
            DepartmentModel d;
            at.ForEach(a =>
            {
                a.DelivDptName = (d = _context.Departments.Find(a.DelivDpt)) == null ? "" : d.Name_C;
            });

            return PartialView(at);
        }

        // GET: BMED/Asset/Details/5
        public IActionResult Details(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            AssetModel asset = _context.BMEDAssets.Find(id);
            if (asset == null)
            {
                return StatusCode(404);
            }
            if (asset.DelivUid != null)
            {
                var userName = _context.AppUsers.Find(asset.DelivUid).UserName;
                asset.DelivEmp = "(" + userName + ") " + asset.DelivEmp;
            }
            asset.DelivDptName = _context.Departments.Find(asset.DelivDpt).Name_C;
            asset.AccDptName = _context.Departments.Find(asset.AccDpt).Name_C;
            asset.VendorName = asset.VendorId == null ? "" : _context.BMEDVendors.Find(asset.VendorId).VendorName;

            return View(asset);
        }

        // GET: BMED/Asset/AssetView/5
        public IActionResult AssetView(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            AssetModel asset = _context.BMEDAssets.Find(id);
            if (asset == null)
            {
                return StatusCode(404);
            }
            asset.DelivDptName = _context.Departments.Find(asset.DelivDpt).Name_C;
            asset.AccDptName = _context.Departments.Find(asset.AccDpt).Name_C;

            return PartialView(asset);
        }

        // GET: BMED/Asset/Create
        public ActionResult Create()
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            _context.Departments.ToList().ForEach(d =>
            {
                listItem.Add(new SelectListItem { Text = d.Name_C, Value = d.DptId });
            });
            ViewData["DelivDpt"] = new SelectList(listItem, "Value", "Text", "");

            List<SelectListItem> listItem2 = new List<SelectListItem>();
            listItem2.Add(new SelectListItem { Text = "", Value = "" });
            ViewData["DelivUid"] = new SelectList(listItem2, "Value", "Text", "");

            ViewData["AccDpt"] = new SelectList(listItem, "Value", "Text", "");

            List<SelectListItem> listItem3 = new List<SelectListItem>();
            listItem3.Add(new SelectListItem { Text = "正常", Value = "正常" });
            listItem3.Add(new SelectListItem { Text = "報廢", Value = "報廢" });
            ViewData["DisposeKind"] = new SelectList(listItem3, "Value", "Text", "");
            //
            List<SelectListItem> listItem4 = new List<SelectListItem>();
            _context.BMEDDeviceClassCodes.ToList()
                .ForEach(d =>
                {
                    listItem4.Add(new SelectListItem { Text = d.M_name, Value = d.M_code });
                });
            ViewData["BmedNo"] = new SelectList(listItem4, "Value", "Text", "");
            //
            // Get MedEngineers to set dropdownlist.
            var s = roleManager.GetUsersInRole("MedEngineer").ToList();
            List<SelectListItem> listItem5 = new List<SelectListItem>();
            foreach (string l in s)
            {
                AppUserModel u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                if (u != null)
                {
                    listItem5.Add(new SelectListItem { Text = u.FullName, Value = u.Id.ToString() });
                }
            }
            ViewData["AssetEngId"] = new SelectList(listItem5, "Value", "Text", "");

            return View();
        }

        // POST: BMED/Asset/Create
        [HttpPost]
        public ActionResult Create(AssetModel asset)
        {
            if (ModelState.IsValid)
            {
                asset.AssetNo = asset.AssetNo.Trim();
                if (_context.BMEDAssets.Find(asset.AssetNo) != null)
                {
                    throw new Exception("財產編號已經存在!!");
                }
                try
                {
                    asset.DelivEmp = asset.DelivUid == null ? "" : _context.AppUsers.Find(asset.DelivUid).FullName;
                    asset.AssetEngName = asset.AssetEngId == 0 ? "" : _context.AppUsers.Find(asset.AssetEngId).FullName;
                    _context.BMEDAssets.Add(asset);
                    AssetKeepModel ak = new AssetKeepModel();
                    ak.AssetNo = asset.AssetNo;
                    _context.BMEDAssetKeeps.Add(ak);
                    _context.SaveChanges();
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }

                return new JsonResult(asset)
                {
                    Value = new { success = true, error = "", id = asset.AssetNo }
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
            //List<SelectListItem> listItem = new List<SelectListItem>();
            //db.Departments.ToList().ForEach(d => {
            //    listItem.Add(new SelectListItem { Text = d.Name_C, Value = d.DptId });
            //});
            //ViewData["DelivDpt"] = new SelectList(listItem, "Value", "Text", "");

            //List<SelectListItem> listItem2 = new List<SelectListItem>();
            //listItem2.Add(new SelectListItem { Text = "", Value = "" });
            //ViewData["DelivUid"] = new SelectList(listItem2, "Value", "Text", "");

            //ViewData["AccDpt"] = new SelectList(listItem, "Value", "Text", "");

            //List<SelectListItem> listItem3 = new List<SelectListItem>();
            //listItem3.Add(new SelectListItem { Text = "正常", Value = "正常" });
            //listItem3.Add(new SelectListItem { Text = "報廢", Value = "報廢" });
            //ViewData["DisposeKind"] = new SelectList(listItem3, "Value", "Text", "");

            //return View(asset);
        }

        // GET: BMED/Asset/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            AssetModel asset = _context.BMEDAssets.Find(id);
            if (asset == null)
            {
                return StatusCode(404);
            }
            //
            List<SelectListItem> listItem = new List<SelectListItem>();
            _context.Departments.ToList().ForEach(d =>
            {
                listItem.Add(new SelectListItem { Text = d.Name_C, Value = d.DptId });
            });
            ViewData["DelivDpt"] = new SelectList(listItem, "Value", "Text", "");

            List<SelectListItem> listItem2 = new List<SelectListItem>();
            _context.AppUsers.Where(u => u.DptId == asset.DelivDpt).ToList().ForEach(u =>
            {
                listItem2.Add(new SelectListItem { Text = u.FullName, Value = u.Id.ToString() });
            });
            if (listItem2.Where(item => item.Value == asset.DelivUid.ToString()).Count() == 0)
                listItem2.Add(new SelectListItem { Text = asset.DelivEmp, Value = asset.DelivUid.ToString() });
            ViewData["DelivUid"] = new SelectList(listItem2, "Value", "Text", "");

            ViewData["AccDpt"] = new SelectList(listItem, "Value", "Text", "");

            List<SelectListItem> listItem3 = new List<SelectListItem>();
            listItem3.Add(new SelectListItem { Text = "正常", Value = "正常" });
            listItem3.Add(new SelectListItem { Text = "報廢", Value = "報廢" });
            ViewData["DisposeKind"] = new SelectList(listItem3, "Value", "Text", "");
            //
            List<SelectListItem> listItem4 = new List<SelectListItem>();
            _context.BMEDDeviceClassCodes.ToList()
                .ForEach(d =>
                {
                    listItem4.Add(new SelectListItem { Text = d.M_name, Value = d.M_code });
                });
            ViewData["BmedNo"] = new SelectList(listItem4, "Value", "Text", "");
            //
            // Get MedEngineers to set dropdownlist.
            var s = roleManager.GetUsersInRole("MedEngineer").ToList();
            List<SelectListItem> listItem5 = new List<SelectListItem>();
            foreach (string l in s)
            {
                AppUserModel u = _context.AppUsers.Where(ur => ur.UserName == l).FirstOrDefault();
                if (u != null)
                {
                    listItem5.Add(new SelectListItem { Text = u.FullName, Value = u.Id.ToString() });
                }
            }
            ViewData["AssetEngId"] = new SelectList(listItem5, "Value", "Text", "");
            //
            if (asset.VendorId != null)
            {
                asset.VendorName = _context.BMEDVendors.Where(v => v.VendorId == asset.VendorId).FirstOrDefault().VendorName;
            }

            return View(asset);
        }

        // POST: BMED/Asset/Edit/5
        [HttpPost]
        public ActionResult Edit(AssetModel asset)
        {
            if (ModelState.IsValid)
            {
                asset.DelivEmp = asset.DelivUid == null ? "" : _context.AppUsers.Find(asset.DelivUid).FullName;
                asset.AssetEngName = asset.AssetEngId == 0 ? "" : _context.AppUsers.Find(asset.AssetEngId).FullName;
                _context.Entry(asset).State = EntityState.Modified;
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                return new JsonResult(asset)
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

        // GET: BMED/Asset/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            AssetModel asset = _context.BMEDAssets.Find(id);
            AssetKeepModel ak = _context.BMEDAssetKeeps.Find(id);
            _context.BMEDAssets.Remove(asset);
            _context.BMEDAssetKeeps.Remove(ak);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // POST: BMED/Asset/UpdEngineer/5
        [HttpPost]
        public ActionResult UpdEngineer(string id, string assets)
        {
            string[] s = assets.Split(new char[] { ';' });
            AssetModel asset;
            foreach (string ss in s)
            {
                asset = _context.BMEDAssets.Find(ss);
                if (asset != null)
                {
                    AppUserModel u = _context.AppUsers.Find(Convert.ToInt32(id));
                    if (u != null)
                    {
                        asset.AssetEngId = u.Id;
                        asset.AssetEngName = u.FullName;
                        _context.Entry(asset).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                }
            }
            return new JsonResult(id)
            {
                Value = new { success = true, error = "" }
            };
        }
    }
}