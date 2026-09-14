using PotyIaApi.Models;

namespace PotyIaApi.Interfaces
{
    public interface IAutenticacaoRepositorio
    {
        /// <summary>
        /// Busca o usuário corporativo (Global.Usuarios) que esteja ativo e
        /// vinculado à aplicação PotyIA (Global.UsuariosAplicacoes + Global.Aplicacoes ativa).
        /// Retorna null quando nenhum registro atende a todos os critérios.
        /// A validação da senha é responsabilidade do Service (PasswordHasher).
        /// </summary>
        Task<UsuarioInternoModel?> BuscarUsuario(string usuario);
        Task<bool> UsuarioExiste(string usuario);
    }
}
