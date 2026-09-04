using PotyIaApi.Interfaces;
using Microsoft.Data.SqlClient;

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

        public SqlConnection BuscarConexaoSenior()
        {
            return new SqlConnection(_configuracao.GetConnectionString("Senior"));
        }

        public void AbrirConexao(SqlConnection con)
        {
            const int maxTentativas = 3;

            for (int tentativa = 1; ; tentativa++)
            {
                try
                {
                    con.Open();
                    return;
                }
                catch (SqlException) when (tentativa < maxTentativas)
                {
                    // Conexao "morta" no pool (ex.: encerrada por firewall/rede apos ociosidade).
                    // Limpa o pool para descartar conexoes invalidas e tenta abrir novamente.
                    SqlConnection.ClearPool(con);
                    Thread.Sleep(200 * tentativa);
                }
            }
        }

        public void FecharConexao(SqlConnection con)
        {
            con.Close();
        }
    }
}
