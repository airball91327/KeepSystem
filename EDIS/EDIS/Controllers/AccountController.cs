using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EDIS.Models;
using EDIS.Models.AccountViewModels;
using EDIS.Services;
using EDIS.Models.Identity;
using System.Net.Http;
using System.Net.Http.Headers;
using EDIS.Areas.BMED.Data;
using EDIS.Data;
using System.Web;
using System.Text;

namespace EDIS.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly CustomUserManager _userManager;
        private readonly CustomSignInManager _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly BMEDDbContext _BMEDcontext;

        public AccountController(
            CustomUserManager userManager,
            CustomSignInManager signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            ApplicationDbContext context,
            BMEDDbContext BMEDcontext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _context = context;
            _BMEDcontext = BMEDcontext;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [NoCache]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(int? docId, string dealType, string returnUrl = null)
        {
            /* If user has already authenticated */
            if (User.Identity.IsAuthenticated == true)
            {
                string MailDocId = docId.ToString();
                string MailType = dealType;
                /* If login from mail. */
                if (MailDocId != "")
                {
                    if (MailType == "Edit") //Edit Repair doc.
                    {
                        var editDoc = _context.RepairFlows.Where(r => r.DocId == MailDocId).OrderByDescending(r => r.StepId)
                                                          .FirstOrDefault();
                        int userId = _context.AppUsers.Where(a => a.UserName == User.Identity.Name).First().Id;
                        /* 編輯流程在登入者身上，進入Edit，否則導回首頁 */
                        if (editDoc.Status == "?" && editDoc.UserId == userId)
                        {
                            return RedirectToAction("Edit", "Repair", new { Area = "", id = MailDocId });
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (MailType == "Views") //View Repair doc.
                    {
                        return RedirectToAction("Views", "Repair", new { Area = "", id = MailDocId });
                    }
                    else if (MailType == "BMEDRepEdit") //Edit BMED Repair Doc.
                    {
                        var editDoc = _BMEDcontext.BMEDRepairFlows.Where(r => r.DocId == MailDocId).OrderByDescending(r => r.StepId)
                                                                  .FirstOrDefault();
                        int userId = _context.AppUsers.Where(a => a.UserName == User.Identity.Name).First().Id;
                        /* 編輯流程在登入者身上，進入Edit，否則導回首頁 */
                        if (editDoc.Status == "?" && editDoc.UserId == userId)
                        {
                            return RedirectToAction("Edit", "Repair", new { Area = "BMED", id = MailDocId });
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (MailType == "BMEDRepViews") //View BMED Repair doc.
                    {
                        return RedirectToAction("Views", "Repair", new { Area = "BMED", id = MailDocId });
                    }
                    else if (MailType == "BMEDKeepEdit") //Edit BMED Keep Doc.
                    {
                        var editDoc = _BMEDcontext.BMEDKeepFlows.Where(r => r.DocId == MailDocId).OrderByDescending(r => r.StepId)
                                                                .FirstOrDefault();
                        int userId = _context.AppUsers.Where(a => a.UserName == User.Identity.Name).First().Id;
                        /* 編輯流程在登入者身上，進入Edit，否則導回首頁 */
                        if (editDoc.Status == "?" && editDoc.UserId == userId)
                        {
                            return RedirectToAction("Edit", "Keep", new { Area = "BMED", id = MailDocId });
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (MailType == "BMEDKeepViews") //View BMED Keep doc.
                    {
                        return RedirectToAction("Views", "Keep", new { Area = "BMED", id = MailDocId });
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }

                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            /* Login from mail. */
            if(docId != null)
            {
                ViewData["MailDocId"] = docId;
                ViewData["MailType"] = dealType;
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            /* Login from mail. */
            ViewData["MailDocId"] = HttpContext.Request.Form["MailDocId"];
            ViewData["MailType"] = HttpContext.Request.Form["MailType"];
            string MailDocId = HttpContext.Request.Form["MailDocId"];
            string MailType = HttpContext.Request.Form["MailType"].ToString();

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                //
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://dms.cch.org.tw:8080/");
                string url = "WebApi/Accounts/CheckPasswdForCch?id=" + model.UserName;
                url += "&pwd=" + HttpUtility.UrlEncode(model.Password, Encoding.GetEncoding("UTF-8"));
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.GetAsync(url);
                string rstr = "";
                if (response.IsSuccessStatusCode)
                {
                    rstr = await response.Content.ReadAsStringAsync();
                }
                client.Dispose();
                //
                //var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                //if (result.Succeeded)
                if(rstr.Contains("成功")) //彰基2000帳號WebApi登入
                {
                    var findUser = _context.AppUsers.Where(a => a.UserName == model.UserName).FirstOrDefault();
                    if(findUser != null)    //AppUsers內搜尋該user detail
                    {
                        var signInId = _context.AppUsers.Where(a => a.UserName == model.UserName).First().Id.ToString();
                        var user = new ApplicationUser { Id = signInId, UserName = model.UserName };

                        await _signInManager.SignInAsync(user, new AuthenticationProperties { IsPersistent = true });

                        /* If login from mail. */
                        if (MailDocId != "")
                        {
                            if (MailType == "Edit")
                            {
                                var editDoc = _context.RepairFlows.Where(r => r.DocId == MailDocId).OrderByDescending(r => r.StepId)
                                                                  .FirstOrDefault();
                                int userId = _context.AppUsers.Where(a => a.UserName == model.UserName).First().Id;
                                /* 編輯流程在登入者身上，進入Edit，否則導回首頁 */
                                if (editDoc.Status == "?" && editDoc.UserId == userId)
                                {
                                    return RedirectToAction(MailType, "Repair", new { Area = "", id = MailDocId });
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                            return RedirectToAction(MailType, "Repair", new { Area = "", id = MailDocId });
                        }

                        _logger.LogInformation("使用者已經登入.");
                        if (!string.IsNullOrEmpty(returnUrl))
                            return RedirectToLocal(returnUrl);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "無此帳號.");
                        return View(model);
                    }
                }
                else  //外包帳號 or 值班帳號
                {
                    /* Check and get external user. */
                    var ExternalUser = _context.ExternalUsers.Where(ex => ex.UserName == model.UserName).FirstOrDefault();
                    if( ExternalUser != null && ExternalUser.Password == model.Password )
                    {
                        var signInId = ExternalUser.Id.ToString();
                        var user = new ApplicationUser { Id = signInId, UserName = model.UserName };

                        await _signInManager.SignInAsync(user, new AuthenticationProperties { IsPersistent = model.RememberMe });

                        /* If login from mail. */
                        if (MailDocId != "")
                        {
                            if (MailType == "Edit")
                            {
                                var editDoc = _context.RepairFlows.Where(r => r.DocId == MailDocId).OrderByDescending(r => r.StepId)
                                                                  .FirstOrDefault();
                                int userId = _context.AppUsers.Where(a => a.UserName == model.UserName).First().Id;
                                /* 編輯流程在登入者身上，進入Edit，否則導回首頁 */
                                if (editDoc.Status == "?" && editDoc.UserId == userId)
                                {
                                    return RedirectToAction(MailType, "Repair", new { Area = "", id = MailDocId });
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                            return RedirectToAction(MailType, "Repair", new { Area = "", id = MailDocId });
                        }

                        _logger.LogInformation("使用者已經登入.");
                        if (!string.IsNullOrEmpty(returnUrl))
                            return RedirectToLocal(returnUrl);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "帳號或密碼錯誤.");
                        return View(model);
                    }
                }
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                //}
                //if (result.IsLockedOut)
                //{
                //    _logger.LogWarning("您的帳號被封鎖.");
                //    return RedirectToAction(nameof(Lockout));
                //}
                //else
                //{
                //    ModelState.AddModelError(string.Empty, "帳號或密碼錯誤.");
                //    return View(model);
                //}
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                //var user = new AppUserModel { UserName = model.Email};
                //var result = await _userManager.CreateAsync(user, model.Password);
                //if (result.Succeeded)
                //{
                //    _logger.LogInformation("User created a new account with password.");

                //    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                //    var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), code, Request.Scheme);
                //    await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                //    await _signInManager.SignInAsync(user, isPersistent: false);
                //    _logger.LogInformation("User created a new account with password.");
                //    return RedirectToLocal(returnUrl);
                //}
                //AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                //var user = new AppUserModel { UserName = model.Email };
                //var result = await _userManager.CreateAsync(user);
                //if (result.Succeeded)
                //{
                //    result = await _userManager.AddLoginAsync(user, info);
                //    if (result.Succeeded)
                //    {
                //        await _signInManager.SignInAsync(user, isPersistent: false);
                //        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                //        return RedirectToLocal(returnUrl);
                //    }
                //}
                //AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code, Request.Scheme);
                //await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                //   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
