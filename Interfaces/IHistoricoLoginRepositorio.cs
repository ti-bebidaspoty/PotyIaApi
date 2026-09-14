namespace PotyIaApi.Interfaces
{
    public interface IHistoricoLoginRepositorio
    {
        public bool UsuarioJaLogou(string usuarioID);
        public void CadastrarPrimeiroLogin(string usuarioID);
    }
}
