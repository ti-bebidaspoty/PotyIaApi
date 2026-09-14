using Microsoft.AspNetCore.Authorization;
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

        public UsuarioController(
            IConfiguration configuration,
            UsuarioService usuarioService)
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

                return retornoApi(
                    null,
                    201,
                    "Usuário criado com sucesso"
                );
            }
            catch (Exception ex)
            {
                return retornoApi(
                    null,
                    500,
                    $"Erro ao criar usuário: {ex.Message}"
                );
            }
        }

        [Authorize]
        [HttpPost]
        [Route("senha")]
        public ActionResult AlterarSenha([FromBody] AlterarSenhaModel model)
        {
            try
            {
                _usuarioService.AlterarSenha(model);

                return retornoApi(
                    null,
                    200,
                    "Senha alterada com sucesso"
                );
            }
            catch (Exception ex)
            {
                return retornoApi(
                    null,
                    500,
                    $"Erro ao alterar senha: {ex.Message}"
                );
            }
        }
    }
}