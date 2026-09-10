using PotyIaApi.Models;

namespace PotyIaApi.Interfaces
{
    public interface IRefreshTokenRepositorio
    {
        void Inserir(RefreshTokenModel refreshToken);
        RefreshTokenModel? BuscarPorHash(string tokenHash);
        void Revogar(long id, string substituidoPorTokenHash);
        void RevogarTodosDoUsuario(string usuarioID);
    }
}
