namespace PotyIaApi.Models
{
    public class UsuarioFormModel
    {
        public required string Nome { get; set; }
        public required string CPF { get; set; }
        public required string Senha { get; set; }
    }

    public class AlterarSenhaModel
    {
        public required string UsuarioID { get; set; }
        public required string NovaSenha { get; set; }
    }
}
