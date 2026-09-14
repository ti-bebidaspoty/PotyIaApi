using PotyIaApi.Interfaces;
using Microsoft.Data.SqlClient;

namespace PotyIaApi.Repositories
{
    public class HistoricoLoginRepositorio : BasicoRepositorio, IHistoricoLoginRepositorio
    {
        public HistoricoLoginRepositorio(IConfiguration configuration) : base(configuration)
        {
        }


        public bool UsuarioJaLogou(string usuarioID)
        {
            SqlConnection con = BuscarConexao();

            try
            {
                AbrirConexao(con);

                string query = @"
            SELECT COUNT(*)
            FROM PotyIA.HistoricosLogins
            WHERE UsuarioID = @UsuarioID";

                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);

                    int quantidade = Convert.ToInt32(cmd.ExecuteScalar());

                    return quantidade > 1;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                FecharConexao(con);
            }
        }

        public void CadastrarPrimeiroLogin(string usuarioID)
        {
            SqlConnection con = BuscarConexao();
            try
            {
                AbrirConexao(con);
                string query = @"INSERT INTO PotyIA.HistoricosLogins (HistoricoLoginID, UsuarioID) VALUES (NEWID(), @UsuarioID)";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                FecharConexao(con);
            }
        }
    }
}
