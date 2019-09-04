// <copyright file="PersonController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Api.Controllers
{
    using Allors.Domain;
    using Api;
    using Allors.Services;

    using Microsoft.AspNetCore.Mvc;

    public class PersonController : Controller
    {
        private readonly ISessionService allors;

        public PersonController(ISessionService allorsContext, ITreeService treeService)
        {
            this.allors = allorsContext;
            this.TreeService = treeService;
        }

        public ITreeService TreeService { get; set; }


        [HttpPost]
        public IActionResult Pull([FromBody] Model model)
        {
            var response = new PullResponseBuilder(this.allors.Session.GetUser(), this.TreeService);

            var person = this.allors.Session.Instantiate(model.Id);
            response.AddObject("person", person);

            return this.Ok(response.Build());
        }

        public class Model
        {
            public string Id { get; set; }
        }
    }
}