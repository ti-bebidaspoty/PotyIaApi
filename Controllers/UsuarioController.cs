using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PotyIaApi.Models;
using PotyIaApi.Services;

namespace PotyIaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : BasicaController
    {
        private readonly IConfiguration _configuracao;
        private readonly UsuarioService _usuarioService;

        public UsuarioController(IConfiguration configuration, UsuarioService usuarioService)
        {
            _configuracao = configuration;
            _usuarioService = usuarioService;
        }

        [HttpPost]
        [Route("{cpf}")]
        public ActionResult CriarUsuario(string cpf)
        {
            try
            {
                _usuarioService.CadastrarUsuario(cpf);
                return retornoApi(null, 201, "Usuário criado com sucesso");
            }
            catch (Exception ex)
            {
                return retornoApi(null, 500, $"Erro ao criar usuário: {ex.Message}");
            }
        }
    }
}
