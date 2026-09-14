using Microsoft.AspNetCore.Identity;
using PotyIaApi.Interfaces;
using PotyIaApi.Models;

namespace PotyIaApi.Services
{
    public class AutenticacaoService
    {
        private readonly IAutenticacaoRepositorio _autenticacaoRepositorio;
        private readonly IPasswordHasher<UsuarioInternoModel> _passwordHasher;
        private readonly ILogger<AutenticacaoService> _logger;
        private readonly IHistoricoLoginRepositorio _historicoLoginRepositorio;

        public AutenticacaoService(
            IAutenticacaoRepositorio autenticacaoRepositorio,
            IPasswordHasher<UsuarioInternoModel> passwordHasher,
            ILogger<AutenticacaoService> logger,
            IHistoricoLoginRepositorio historicoLoginRepositorio)
        {
            _autenticacaoRepositorio = autenticacaoRepositorio;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _historicoLoginRepositorio = historicoLoginRepositorio;
        }

        public async Task<ResultadoAutenticacaoModel> RealizarAutenticacao(
            AutenticacaoModel autenticacao)
        {
            if (autenticacao == null ||
                string.IsNullOrWhiteSpace(autenticacao.Usuario) ||
                string.IsNullOrWhiteSpace(autenticacao.Senha))
            {
                _logger.LogInformation(
                    "Tentativa de login com credenciais vazias."
                );

                return new ResultadoAutenticacaoModel
                {
                    Sucesso = false,
                    UsuarioNaoCadastrado = false
                };
            }

            // Primeiro verifica se o CPF realmente existe em Global.Usuarios.
            var usuarioExiste =
                await _autenticacaoRepositorio.UsuarioExiste(
                    autenticacao.Usuario
                );

            if (!usuarioExiste)
            {
                _logger.LogInformation(
                    "Login negado para '{Usuario}': usuário ainda não cadastrado.",
                    autenticacao.Usuario
                );

                return new ResultadoAutenticacaoModel
                {
                    Sucesso = false,
                    UsuarioNaoCadastrado = true
                };
            }

            // Aqui o CPF existe, então verificamos se está ativo,
            // vinculado ao PotyIA e com a aplicação ativa.
            var usuarioInterno =
                await _autenticacaoRepositorio.BuscarUsuario(
                    autenticacao.Usuario
                );

            if (usuarioInterno == null)
            {
                _logger.LogInformation(
                    "Login negado para '{Usuario}': usuário inativo ou sem vínculo com o PotyIA.",
                    autenticacao.Usuario
                );

                return new ResultadoAutenticacaoModel
                {
                    Sucesso = false,
                    UsuarioNaoCadastrado = false
                };
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuarioInterno,
                usuarioInterno.SenhaHash,
                autenticacao.Senha
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                _logger.LogInformation(
                    "Login negado para '{Usuario}': senha inválida.",
                    autenticacao.Usuario
                );

                return new ResultadoAutenticacaoModel
                {
                    Sucesso = false,
                    UsuarioNaoCadastrado = false
                };
            }

            _historicoLoginRepositorio.CadastrarPrimeiroLogin(
                usuarioInterno.UsuarioID
            );

            var usuarioAutenticado = new UsuarioAutenticadoModel
            {
                UsuarioID = usuarioInterno.UsuarioID,
                Nome = usuarioInterno.Nome,
                Usuario = usuarioInterno.Usuario
            };

            return new ResultadoAutenticacaoModel
            {
                Sucesso = true,
                UsuarioNaoCadastrado = false,
                Usuario = usuarioAutenticado
            };
        }
    }
}