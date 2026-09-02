namespace PotyIaApi.Models
{
    /// <summary>
    /// Entidade que representa um Refresh Token armazenado no banco.
    /// Apenas o hash do token é persistido (TokenHash), nunca o valor puro.
    /// </summary>
    public class RefreshTokenModel
    {
        public long Id { get; set; }
        public required string TokenHash { get; set; }
        public required string UsuarioID { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime ExpiraEm { get; set; }
        public DateTime? RevogadoEm { get; set; }
        public string? SubstituidoPorTokenHash { get; set; }

        public bool Expirado => DateTime.UtcNow >= ExpiraEm;
        public bool Revogado => RevogadoEm != null;
        public bool Ativo => !Revogado && !Expirado;
    }

    /// <summary>
    /// Corpo esperado no endpoint POST api/Autenticacao/refresh-token.
    /// </summary>
    public class RefreshTokenRequestModel
    {
        public required string RefreshToken { get; set; }
    }

    /// <summary>
    /// Retorno padrão dos fluxos que emitem tokens (login e refresh).
    /// </summary>
    public class TokensRespostaModel
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiraEm { get; set; }
    }
}
