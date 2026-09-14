using PotyIaApi.Interfaces;

namespace PotyIaApi.Services
{
    public class HistoricoLoginService
    {
        private readonly IHistoricoLoginRepositorio _historicoLoginRepositorio;

        public HistoricoLoginService(IHistoricoLoginRepositorio historicoLoginRepositorio)
        {
            _historicoLoginRepositorio = historicoLoginRepositorio;
        }

        public bool UsuarioJaLogou(string usuarioID)
        {
            return _historicoLoginRepositorio.UsuarioJaLogou(usuarioID);
        }

        public void CadastrarPrimeiroLogin(string usuarioID)
        {
            _historicoLoginRepositorio.CadastrarPrimeiroLogin(usuarioID);
        }
    }
}
