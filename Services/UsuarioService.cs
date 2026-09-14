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

            usuario.Nome = FormatarNome(usuario.Nome);

            usuario.Senha = _passwordHasher.HashPassword(
                new UsuarioInternoModel(),
                usuario.Senha
            );

            var usuarioID = _usuarioRepositorio.CadastrarUsuario(usuario);

            _usuarioRepositorio.CadastrarUsuarioAplicacao(usuarioID);
        }

        public void AlterarSenha(AlterarSenhaModel model)
        {
            if (string.IsNullOrWhiteSpace(model.UsuarioID))
            {
                throw new Exception("Usuário não informado.");
            }

            if (string.IsNullOrWhiteSpace(model.NovaSenha))
            {
                throw new Exception("Nova senha não informada.");
            }

            string senhaHash = _passwordHasher.HashPassword(
                new UsuarioInternoModel(),
                model.NovaSenha
            );

            bool alterado = _usuarioRepositorio.AlterarSenha(
                model.UsuarioID,
                senhaHash
            );

            if (!alterado)
            {
                throw new Exception("Usuário não encontrado.");
            }
        }

        private UsuarioFormModel VerificarUsuarioSenior(string cpf)
        {
            var usuario = _usuarioRepositorio.VerificarUsuarioSenior(cpf);

            if (usuario == null)
            {
                throw new Exception("Usuário não encontrado.");
            }

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

            return cultura.TextInfo.ToTitleCase(
                nome.Trim().ToLower(cultura)
            );
        }
    }
}