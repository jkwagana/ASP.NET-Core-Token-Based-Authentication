using HomeAPIs.Data.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TwitterAPIs.Controllers
{

    [Authorize(Roles = UserRoles.Student)]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        public StudentsController() { }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("welcome to studentController");
        }
    }
}
//Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False