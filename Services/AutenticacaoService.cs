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

        public AutenticacaoService(
            IAutenticacaoRepositorio autenticacaoRepositorio,
            IPasswordHasher<UsuarioInternoModel> passwordHasher,
            ILogger<AutenticacaoService> logger)
        {
            _autenticacaoRepositorio = autenticacaoRepositorio;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        /// <summary>
        /// Autentica o usuário corporativo (schema Global) vinculado ao PotyIA.
        /// Retorna null em qualquer falha; o motivo é apenas registrado em log,
        /// nunca exposto ao cliente (evita enumeração de usuários).
        /// </summary>
        public async Task<UsuarioAutenticadoModel?> RealizarAutenticacao(AutenticacaoModel autenticacao)
        {
            if (autenticacao == null ||
                string.IsNullOrWhiteSpace(autenticacao.Usuario) ||
                string.IsNullOrWhiteSpace(autenticacao.Senha))
            {
                _logger.LogInformation("Tentativa de login com credenciais vazias.");
                return null;
            }

            // O repositório já filtra: usuário ativo, vínculo com a aplicação
            // PotyIA e aplicação ativa. Se vier null, uma dessas condições falhou.
            var usuarioInterno = await _autenticacaoRepositorio.BuscarUsuario(autenticacao.Usuario);

            if (usuarioInterno == null)
            {
                _logger.LogInformation(
                    "Login negado para '{Usuario}': usuário inexistente, inativo ou sem vínculo com o PotyIA.",
                    autenticacao.Usuario);
                return null;
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuarioInterno,
                usuarioInterno.SenhaHash,
                autenticacao.Senha);

            if (resultado == PasswordVerificationResult.Failed)
            {
                _logger.LogInformation(
                    "Login negado para '{Usuario}': senha inválida.",
                    autenticacao.Usuario);
                return null;
            }

            // Success e SuccessRehashNeeded são aceitos. O PotyIA NÃO atualiza o
            // hash: a manutenção da senha pertence ao PotyInternos.
            return new UsuarioAutenticadoModel
            {
                UsuarioID = usuarioInterno.UsuarioID,
                Nome = usuarioInterno.Nome,
                Usuario = usuarioInterno.Usuario
            };
        }
    }
}
