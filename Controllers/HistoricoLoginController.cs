using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PotyIaApi.Services;

namespace PotyIaApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class HistoricoLoginController : BasicaController
    {
        private readonly IConfiguration _configuracao;
        private readonly HistoricoLoginService _historicoLoginService;

        public HistoricoLoginController(IConfiguration configuration, HistoricoLoginService historicoLoginService)
        {
            _configuracao = configuration;
            _historicoLoginService = historicoLoginService;
        }

        [HttpGet("{usuarioID}")]
        public IActionResult UsuarioJaLogou(string usuarioID)
        {
            bool jaLogou = _historicoLoginService.UsuarioJaLogou(usuarioID);
            return Ok(jaLogou);
        }
    }
}
