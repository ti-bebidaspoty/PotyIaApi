using PotyIaApi.Interfaces;
using PotyIaApi.Models;

namespace PotyIaApi.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public UsuarioService(IUsuarioRepositorio usuarioRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
        }

        public void CadastrarUsuario(string cpf)
        {
            if (VerificarUsuarioJaCadastrado(cpf))
            {
                throw new Exception("Usuário já cadastrado.");
            }

            UsuarioFormModel usuario = VerificarUsuarioSenior(cpf);

            if (usuario == null)
            {
                throw new Exception("Usuário não encontrado.");
            }

            _usuarioRepositorio.CadastrarUsuario(usuario);
        }

        private UsuarioFormModel VerificarUsuarioSenior(string cpf)
        {
            var usuario = _usuarioRepositorio.VerificarUsuarioSenior(cpf);
            if (usuario == null)
            {
                throw new Exception("Usuário não encontrado.");
            };

            return usuario;
        }

        private bool VerificarUsuarioJaCadastrado(string cpf)
        {
            return _usuarioRepositorio.VerificarUsuarioJaCadastrado(cpf);
        }
    }
}
