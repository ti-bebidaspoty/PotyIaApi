using Microsoft.AspNetCore.Mvc;
using PotyIaApi.Services;

namespace PotyIaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadAppController : BasicaController
    {
        private readonly DownloadAppService _downloadAppService;

        public DownloadAppController(DownloadAppService downloadAppService)
        {
            _downloadAppService = downloadAppService;
        }

        [HttpGet("")]
        public IActionResult Download()
        {
            var userAgent = Request.Headers.UserAgent.ToString();

            var resultado = _downloadAppService.DownloadApp(userAgent);

            if (resultado.Status == DownloadAppStatus.SemLinksDisponiveis)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    "Não existem mais links disponíveis para iOS."
                );
            }

            return Redirect(resultado.Link!);
        }
    }
}