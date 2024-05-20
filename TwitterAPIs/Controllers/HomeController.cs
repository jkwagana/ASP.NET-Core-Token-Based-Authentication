using HomeAPIs.Data.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TwitterAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.Student + "-" + UserRoles.Manager)]
    public class HomeController : ControllerBase
    {
        public HomeController() { }


        [HttpGet("student")]
        [Authorize(Roles = UserRoles.Student)]
        public IActionResult GetStudents()
        {
            return Ok("welcome to the HomeController - student");

        }
        [HttpGet("manager")]
        [Authorize(Roles = UserRoles.Manager)]
        public IActionResult GetManager()
        {
            return Ok("welcome to the HomeController - manager");

        }
    }
}
