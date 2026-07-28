using PotyIaApi.Models;

namespace PotyIaApi.Interfaces
{
    public interface IAutenticacaoRepositorio
    {
        public UsuarioAutenticadoModel RealizarAutenticacao(AutenticacaoModel autenticacao);

    }
}
