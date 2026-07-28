using System.Data.SqlClient;

namespace PotyIaApi.Interfaces
{
    public interface IBasicoRepositorio
    {
        SqlConnection BuscarConexao();
        void AbrirConexao(SqlConnection con);
        void FecharConexao(SqlConnection con);
    }
}
