using PotyIaApi.Interfaces;
using PotyIaApi.Models;
using System.Security.Cryptography;

namespace PotyIaApi.Services
{
    /// <summary>
    /// Resultado da tentativa de renovação de tokens.
    /// Sucesso indica se o refresh foi aceito; erros são sempre genéricos.
    /// </summary>
    public class ResultadoRefreshToken
    {
        public bool Sucesso { get; init; }
        public TokensRespostaModel? Tokens { get; init; }
    }

    public class RefreshTokenService
    {
        private readonly IConfiguration _config;
        private readonly IRefreshTokenRepositorio _refreshTokenRepositorio;
        private readonly IHelper _helper;
        private readonly TokenService _tokenService;

        public RefreshTokenService(
            IConfiguration config,
            IRefreshTokenRepositorio refreshTokenRepositorio,
            IHelper helper,
            TokenService tokenService)
        {
            _config = config;
            _refreshTokenRepositorio = refreshTokenRepositorio;
            _helper = helper;
            _tokenService = tokenService;
        }

        // ==================================================
        // LOGIN: gera access + refresh
        // ==================================================

        public TokensRespostaModel GerarTokens(UsuarioAutenticadoModel usuario)
        {
            var accessToken = _tokenService.GerarAccessToken(usuario);
            var (refreshTokenPuro, entidade) = CriarRefreshToken(usuario.UsuarioID);

            _refreshTokenRepositorio.Inserir(entidade);

            return new TokensRespostaModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenPuro,
                RefreshTokenExpiraEm = entidade.ExpiraEm
            };
        }

        // ==================================================
        // REFRESH: valida, rotaciona e emite novos tokens
        // ==================================================

        public ResultadoRefreshToken RenovarTokens(string refreshTokenPuro)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenPuro))
                return new ResultadoRefreshToken { Sucesso = false };

            var tokenHash = _helper.Criptografar(refreshTokenPuro);
            var armazenado = _refreshTokenRepositorio.BuscarPorHash(tokenHash);

            // Token inexistente.
            if (armazenado == null)
                return new ResultadoRefreshToken { Sucesso = false };

            // Proteção contra reutilização: token já revogado sendo reapresentado.
            // Revoga toda a cadeia do usuário como medida de segurança.
            if (armazenado.Revogado)
            {
                _refreshTokenRepositorio.RevogarTodosDoUsuario(armazenado.UsuarioID);
                return new ResultadoRefreshToken { Sucesso = false };
            }

            // Expirado.
            if (armazenado.Expirado)
                return new ResultadoRefreshToken { Sucesso = false };

            // Usuário dono do refresh token (a partir da própria entidade, fonte confiável).
            var usuario = new UsuarioAutenticadoModel
            {
                UsuarioID = armazenado.UsuarioID,
                Nome = string.Empty
            };

            // Rotação: cria novo refresh token e revoga o antigo.
            var (novoRefreshPuro, novaEntidade) = CriarRefreshToken(armazenado.UsuarioID);
            _refreshTokenRepositorio.Inserir(novaEntidade);
            _refreshTokenRepositorio.Revogar(armazenado.Id, novaEntidade.TokenHash);

            var novoAccessToken = _tokenService.GerarAccessToken(usuario);

            return new ResultadoRefreshToken
            {
                Sucesso = true,
                Tokens = new TokensRespostaModel
                {
                    AccessToken = novoAccessToken,
                    RefreshToken = novoRefreshPuro,
                    RefreshTokenExpiraEm = novaEntidade.ExpiraEm
                }
            };
        }

        // ==================================================
        // AUXILIARES
        // ==================================================

        private (string tokenPuro, RefreshTokenModel entidade) CriarRefreshToken(string usuarioID)
        {
            var tokenPuro = GerarTokenAleatorio();
            var dias = int.TryParse(_config["Jwt:RefreshTokenExpirationDays"], out var d) ? d : 7;

            var entidade = new RefreshTokenModel
            {
                TokenHash = _helper.Criptografar(tokenPuro),
                UsuarioID = usuarioID,
                CriadoEm = DateTime.UtcNow,
                ExpiraEm = DateTime.UtcNow.AddDays(dias)
            };

            return (tokenPuro, entidade);
        }

        private static string GerarTokenAleatorio()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }
    }
}
