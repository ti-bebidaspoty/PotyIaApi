using Microsoft.AspNetCore.Mvc;
using PotyIaApi.Models;

namespace PotyIaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasicaController : ControllerBase
    {
        [NonAction]
        public ActionResult retornoApi(Object? retorno, int codigoErro = 0, string mensagemErro = "")
        {
            if (retorno == null)
            {
                var response = new ResponseModel
                {
                    Status = codigoErro,
                    Resposta = mensagemErro
                };
                return StatusCode(codigoErro, response);
            }

            return StatusCode(200, retorno);
        }
    }
}
