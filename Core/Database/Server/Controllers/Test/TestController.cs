using Microsoft.Extensions.Logging;

namespace Allors.Server.Controllers
{
    using System;
    using Allors.Domain;
    using Allors.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;

    public class TestController : Controller
    {
        public TestController(IDatabaseService databaseService)
        {
            this.Database = databaseService.Database;
        }

        public IDatabase Database { get; set; }

        private ILogger<TestController> Logger { get; set; }

        [HttpGet]
        public IActionResult Ready()
        {
            return this.Ok("Ready");
        }

        [HttpGet]
        public IActionResult Init()
        {
            try
            {
                var stateService = this.Database.ServiceProvider.GetRequiredService<IStateService>();

                var database = this.Database;
                database.Init();
                stateService.Clear();

                return this.Ok("Init");
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Exception");
                return BadRequest(e);
            }
        }

        [HttpGet]
        public IActionResult Setup(string population)
        {
            try
            {
                var stateService = this.Database.ServiceProvider.GetRequiredService<IStateService>();

                var database = this.Database;
                database.Init();
                stateService.Clear();

                using (var session = database.CreateSession())
                {
                    var config = new Config();
                    new Setup(session, config).Apply();
                    session.Derive();
                    session.Commit();

                    var administrator = new Users(session).GetUser("administrator");
                    session.SetUser(administrator);

                    new TestPopulation(session, population).Apply();
                    session.Derive();
                    session.Commit();
                }

                return this.Ok("Setup");
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Exception");
                return BadRequest(e);
            }
        }

        [HttpGet]
        public IActionResult TimeShift(int days, int hours = 0, int minutes = 0, int seconds = 0)
        {
            try
            {
                var timeService = this.Database.ServiceProvider.GetRequiredService<ITimeService>();
                timeService.Shift = new TimeSpan(days, hours, minutes, seconds);
                return this.Ok();
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Exception");
                return BadRequest(e);
            }
        }
    }
}