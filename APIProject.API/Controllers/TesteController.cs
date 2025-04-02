using Microsoft.AspNetCore.Mvc;

namespace APIProject.API.Controllers
{
    [Route("api/teste")]
    [ApiController]
    public class TesteController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get()
        {
            return Ok("Controller de teste funcionando!");
        }


    }
}
