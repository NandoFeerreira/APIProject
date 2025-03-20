using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TesteController : ControllerBase
    {
        [HttpGet("anonimo")]
        [AllowAnonymous]
        public IActionResult Anonimo()
        {
            return Ok(new { mensagem = "Endpoint anônimo acessado com sucesso!" });
        }

        [HttpGet("autenticado")]
        [Authorize]
        public IActionResult Autenticado()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            return Ok(new { 
                mensagem = "Endpoint autenticado acessado com sucesso!",
                usuario = new {
                    id = userId,
                    nome = userName,
                    email = userEmail
                }
            });
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return Ok(new { mensagem = "Endpoint de administrador acessado com sucesso!" });
        }
    }
}