﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Server
{
    using System.Threading.Tasks;

    using Allors.Services;

    using Identity.Models;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class AuthenticationController : Controller
    {
        public AuthenticationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AuthenticationController> logger, IConfiguration config, ISessionService sessionService)
        {
            this.UserManager = userManager;
            this.SignInManager = signInManager;
            this.Logger = logger;
            this.Configuration = config;
            this.SessionService = sessionService;
        }

        public UserManager<ApplicationUser> UserManager { get; }

        public SignInManager<ApplicationUser> SignInManager { get; }

        public ILogger Logger { get; }

        public IConfiguration Configuration { get; }

        public ISessionService SessionService { get; }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Token([FromBody]AuthenticationTokenRequest request)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this.UserManager.FindByNameAsync(request.UserName);

                if (user != null)
                {
                    var result = await this.SignInManager.CheckPasswordSignInAsync(user, request.Password, false);
                    if (result.Succeeded)
                    {

                        var token = user.CreateToken(this.Configuration);
                        var response = new AuthenticationTokenResponse
                        {
                            Authenticated = true,
                            UserId = user.Id,
                            Token = token
                        };
                        return this.Ok(response);
                    }
                }
            }

            return this.Ok(new { Authenticated = false });
        }

    }
}
