using HomeAPIs.Data.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TwitterAPIs.Controllers
{


    [Authorize(Roles = UserRoles.Manager)]
    [Route("api/[controller]")]
    [ApiController]
    public class ManagementController : ControllerBase
    {
        public ManagementController() { }


        [HttpGet]
        public IActionResult Get()
        {
            return Ok("welcome to managementController");

        }

    }


}
