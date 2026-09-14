namespace PotyIaApi.Models
{
    public class AutenticacaoModel
    {
        public required string Usuario { get; set; }
        public required string Senha { get; set; }
    }

    public class UsuarioAutenticadoModel
    {
        public required string UsuarioID { get; set; }
        public required string Nome { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    /// <summary>
    /// Representa o usuário corporativo obtido do schema Global (PotyInternos).
    /// Usado internamente para validar a senha via PasswordHasher.
    /// A senha (hash) nunca é exposta na resposta da API.
    /// </summary>
    public class UsuarioInternoModel
    {
        public string UsuarioID { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public bool Status { get; set; }
    }

    public class ResultadoAutenticacaoModel
    {
        public bool Sucesso { get; set; }

        public bool UsuarioNaoCadastrado { get; set; }

        public UsuarioAutenticadoModel? Usuario { get; set; }
    }
}
