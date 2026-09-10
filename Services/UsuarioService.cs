using Microsoft.AspNetCore.Identity;
using PotyIaApi.Interfaces;
using PotyIaApi.Models;
using System.Globalization;

namespace PotyIaApi.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IPasswordHasher<UsuarioInternoModel> _passwordHasher;

        public UsuarioService(
            IUsuarioRepositorio usuarioRepositorio,
            IPasswordHasher<UsuarioInternoModel> passwordHasher)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _passwordHasher = passwordHasher;
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

            // O nome vem da Senior todo em maiúsculo. Formata para Title Case
            // (ex.: "JOAO PEDRO MARTINS" -> "Joao Pedro Martins").
            usuario.Nome = FormatarNome(usuario.Nome);

            // Gera o hash da senha com o MESMO algoritmo usado na autenticação
            // (Microsoft.AspNetCore.Identity.PasswordHasher), garantindo que
            // VerifyHashedPassword valide corretamente no login.
            usuario.Senha = _passwordHasher.HashPassword(new UsuarioInternoModel(), usuario.Senha);

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

        private static string FormatarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return nome;

            var cultura = new CultureInfo("pt-BR");
            return cultura.TextInfo.ToTitleCase(nome.Trim().ToLower(cultura));
        }
    }
}
