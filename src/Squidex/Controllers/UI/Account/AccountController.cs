﻿// ==========================================================================
//  AccountController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using Microsoft.Extensions.Options;
using Squidex.Config;
using Squidex.Config.Identity;
using Squidex.Core.Identity;
using Squidex.Infrastructure.Tasks;

// ReSharper disable InvertIf
// ReSharper disable RedundantIfElseBlock
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Squidex.Controllers.UI.Account
{
    [SwaggerIgnore]
    public sealed class AccountController : Controller
    {
        private static readonly EventId IdentityEventId = new EventId(8000, "IdentityEventId");
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IOptions<MyIdentityOptions> identityOptions;
        private readonly IOptions<MyUrlsOptions> urlOptions;
        private readonly ILogger<AccountController> logger;
        private readonly IIdentityServerInteractionService interactions;

        public AccountController(
            SignInManager<IdentityUser> signInManager, 
            UserManager<IdentityUser> userManager,
            IOptions<MyIdentityOptions> identityOptions,
            IOptions<MyUrlsOptions> urlOptions,
            ILogger<AccountController> logger,
            IIdentityServerInteractionService interactions)
        {
            this.logger = logger;
            this.urlOptions = urlOptions;
            this.userManager = userManager;
            this.interactions = interactions;
            this.identityOptions = identityOptions;
            this.signInManager = signInManager;
        }
        
        [HttpGet]
        [Route("account/forbidden")]
        public IActionResult Forbidden()
        {
            return View("Error");
        }
        
        [HttpGet]
        [Route("account/accessdenied")]
        public IActionResult AccessDenied()
        {
            return View("LockedOut");
        }

        [HttpGet]
        [Route("client-callback-silent/")]
        public IActionResult ClientSilent()
        {
            return View();
        }

        [HttpGet]
        [Route("client-callback-popup/")]
        public IActionResult ClientPopup()
        {
            return View();
        }

        [HttpGet]
        [Route("account/logout-completed/")]
        public IActionResult LogoutCompleted()
        {
            return View();
        }

        [HttpGet]
        [Route("account/error/")]
        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        [Route("account/logout/")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var context = await interactions.GetLogoutContextAsync(logoutId);
            
            await signInManager.SignOutAsync();

            var logoutUrl = context.PostLogoutRedirectUri;

            if (string.IsNullOrWhiteSpace(logoutUrl))
            {
                logoutUrl = urlOptions.Value.BuildUrl("logout");
            }

            return Redirect(logoutUrl);
        }

        [HttpGet]
        [Route("account/logout-redirect/")]
        public async Task<IActionResult> LogoutRedirect()
        {
            await signInManager.SignOutAsync();

            return RedirectToAction(nameof(LogoutCompleted));
        }

        [HttpGet]
        [Route("account/login/")]
        public IActionResult Login(string returnUrl = null)
        {
            var providers = 
                signInManager.GetExternalAuthenticationSchemes()
                    .Select(x => new ExternalProvider(x.AuthenticationScheme, x.DisplayName)).ToList();

            return View(new LoginVM { ExternalProviders = providers, ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("account/external/")]
        public IActionResult External(string provider, string returnUrl = null)
        {
            var properties = 
                signInManager.ConfigureExternalAuthenticationProperties(provider,
                    Url.Action(nameof(Callback), new { ReturnUrl = returnUrl }));

            return Challenge(properties, provider);
        }

        [HttpGet]
        [Route("account/callback/")]
        public async Task<IActionResult> Callback(string returnUrl = null, string remoteError = null)
        {
            var externalLogin = await signInManager.GetExternalLoginInfoAsync();

            if (externalLogin == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await signInManager.ExternalLoginSignInAsync(externalLogin.LoginProvider, externalLogin.ProviderKey, true);

            if (!result.Succeeded && result.IsLockedOut)
            {
                return View("LockedOut");
            }

            var isLoggedIn = result.Succeeded;

            if (!isLoggedIn)
            {
                var user = CreateUser(externalLogin);

                var isFirst = userManager.Users.LongCount() == 0;

                isLoggedIn =
                    await AddUserAsync(user) &&
                    await AddLoginAsync(user, externalLogin) &&
                    await MakeAdminAsync(user, isFirst) &&
                    await LockAsync(user, isFirst) &&
                    await LoginAsync(externalLogin);
            }

            if (!isLoggedIn)
            {
                return RedirectToAction(nameof(Login));
            }
            else if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("~/");
            }
        }

        private Task<bool> AddLoginAsync(IdentityUser user, UserLoginInfo externalLogin)
        {
            return MakeIdentityOperation(() => userManager.AddLoginAsync(user, externalLogin));
        }

        private Task<bool> AddUserAsync(IdentityUser user)
        {
            return MakeIdentityOperation(() => userManager.CreateAsync(user));
        }

        private async Task<bool> LoginAsync(UserLoginInfo externalLogin)
        {
            var result = await signInManager.ExternalLoginSignInAsync(externalLogin.LoginProvider, externalLogin.ProviderKey, true);

            return result.Succeeded;
        }

        private Task<bool> LockAsync(IdentityUser user, bool isFirst)
        {
            if (isFirst || !identityOptions.Value.LockAutomatically)
            {
                return TaskHelper.True;
            }

            return MakeIdentityOperation(() => userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100)));
        }

        private Task<bool> MakeAdminAsync(IdentityUser user, bool isFirst)
        {
            if (!isFirst)
            {
                return TaskHelper.True;
            }

            return MakeIdentityOperation(() => userManager.AddToRoleAsync(user, SquidexRoles.Administrator));
        }

        private static IdentityUser CreateUser(ExternalLoginInfo externalLogin)
        {
            var mail = externalLogin.Principal.FindFirst(ClaimTypes.Email).Value;

            var user = new IdentityUser { Email = mail, UserName = mail };

            foreach (var squidexClaim in externalLogin.Principal.Claims.Where(c => c.Type.StartsWith(SquidexClaimTypes.Prefix)))
            {
                user.AddClaim(squidexClaim);
            }

            return user;
        }

        private async Task<bool> MakeIdentityOperation(Func<Task<IdentityResult>> action, [CallerMemberName] string operationName = null)
        {
            try
            {
                var result = await action();

                if (!result.Succeeded)
                {
                    var errorMessageBuilder = new StringBuilder();

                    foreach (var error in result.Errors)
                    {
                        errorMessageBuilder.Append(error.Code);
                        errorMessageBuilder.Append(": ");
                        errorMessageBuilder.AppendLine(error.Description);
                    }

                    logger.LogError(IdentityEventId, "Operation '{0}' failed with errors: {1}", operationName, errorMessageBuilder.ToString());
                }

                return result.Succeeded;
            }
            catch (Exception e)
            {
                logger.LogError(IdentityEventId, e, "Operation '{0}' failed with exception", operationName);

                return false;
            }
        }
    }
}
