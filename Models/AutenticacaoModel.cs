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
    }
}
