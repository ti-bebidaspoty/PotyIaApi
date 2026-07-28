using PotyIaApi.Interfaces;
using PotyIaApi.Models;

namespace PotyIaApi.Services
{
    public class AutenticacaoService
    {
        private readonly IAutenticacaoRepositorio _autenticacaoRepositorio;

        public AutenticacaoService(IAutenticacaoRepositorio autenticacaoRepositorio)
        {
            _autenticacaoRepositorio = autenticacaoRepositorio;
        }

        public UsuarioAutenticadoModel RealizarAutenticacao(AutenticacaoModel autenticacao)
        {
            UsuarioAutenticadoModel usuarioAutenticado = null!;

            try
            {
                usuarioAutenticado = _autenticacaoRepositorio.RealizarAutenticacao(autenticacao);
            }
            catch (Exception)
            {
                throw;
            }

            return usuarioAutenticado;
        }
    }
}
