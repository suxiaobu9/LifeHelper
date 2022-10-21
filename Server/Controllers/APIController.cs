using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeHelper.Server.Controllers
{
    [Route("api")]
    [ApiController]
    public class APIController : ControllerBase
    {
        [HttpPost("WakeUp")]
        public string WakeUp()
        {
            return "I woke up!";
        }
    }
}
