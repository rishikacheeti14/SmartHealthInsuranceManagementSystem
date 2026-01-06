using Microsoft.AspNetCore.Mvc;
using SmartHealthInsurance.Api.Data;

namespace SmartHealthInsurance.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public TestingController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpPost("reset-data")]
        public IActionResult ResetData()
        {
            DataSeeder.ClearAllDataExceptAdmin(_serviceProvider);
            return Ok(new { message = "All application data (except Admin) has been cleared." });
        }
    }
}
