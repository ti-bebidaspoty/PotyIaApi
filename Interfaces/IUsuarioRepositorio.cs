using PotyIaApi.Models;

namespace PotyIaApi.Interfaces
{
    public interface IUsuarioRepositorio
    {
        public void CadastrarUsuario(UsuarioFormModel usuario);
        public bool VerificarUsuarioJaCadastrado(string cpf);
        public UsuarioFormModel VerificarUsuarioSenior(string cpf);
    }
}
