using PotyIaApi.Models;

namespace PotyIaApi.Interfaces
{
    public interface IUsuarioRepositorio
    {
        public string CadastrarUsuario(UsuarioFormModel usuario);
        public bool VerificarUsuarioJaCadastrado(string cpf);
        public UsuarioFormModel VerificarUsuarioSenior(string cpf);
        public void CadastrarUsuarioAplicacao(string usuarioID);
        bool AlterarSenha(string usuarioID, string senha);
    }
}
