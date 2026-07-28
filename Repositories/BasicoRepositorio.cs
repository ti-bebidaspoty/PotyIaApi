using PotyIaApi.Interfaces;
using System.Data.SqlClient;

namespace PotyIaApi.Repositories
{
    public class BasicoRepositorio: IBasicoRepositorio
    {
        private readonly IConfiguration _configuracao;

        public BasicoRepositorio(IConfiguration configuration)
        {
            _configuracao = configuration;
        }

        public SqlConnection BuscarConexao()
        {
            return new SqlConnection(_configuracao.GetConnectionString("InternosPoty"));
        }

        public void AbrirConexao(SqlConnection con)
        {
            con.Open();
        }

        public void FecharConexao(SqlConnection con)
        {
            con.Close();
        }
    }
}
