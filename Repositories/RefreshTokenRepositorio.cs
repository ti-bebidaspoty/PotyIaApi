using PotyIaApi.Interfaces;
using PotyIaApi.Models;
using Microsoft.Data.SqlClient;

namespace PotyIaApi.Repositories
{
    public class RefreshTokenRepositorio : BasicoRepositorio, IRefreshTokenRepositorio
    {
        public RefreshTokenRepositorio(IConfiguration configuration) : base(configuration)
        {
        }

        public void Inserir(RefreshTokenModel refreshToken)
        {
            SqlConnection con = BuscarConexao();

            try
            {
                AbrirConexao(con);

                string query = @"INSERT INTO PotyIA.RefreshTokens
                                    (TokenHash, UsuarioID, CriadoEm, ExpiraEm)
                                 VALUES
                                    (@TokenHash, @UsuarioID, @CriadoEm, @ExpiraEm)";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@TokenHash", refreshToken.TokenHash);
                cmd.Parameters.AddWithValue("@UsuarioID", refreshToken.UsuarioID);
                cmd.Parameters.AddWithValue("@CriadoEm", refreshToken.CriadoEm);
                cmd.Parameters.AddWithValue("@ExpiraEm", refreshToken.ExpiraEm);
                cmd.ExecuteNonQuery();
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

        public RefreshTokenModel? BuscarPorHash(string tokenHash)
        {
            SqlConnection con = BuscarConexao();
            RefreshTokenModel? refreshToken = null;

            try
            {
                AbrirConexao(con);

                string query = @"SELECT Id, TokenHash, UsuarioID, CriadoEm, ExpiraEm,
                                        RevogadoEm, SubstituidoPorTokenHash
                                 FROM PotyIA.RefreshTokens
                                 WHERE TokenHash = @TokenHash";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@TokenHash", tokenHash);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    refreshToken = new RefreshTokenModel
                    {
                        Id = Convert.ToInt64(reader["Id"]),
                        TokenHash = reader["TokenHash"].ToString()!,
                        UsuarioID = reader["UsuarioID"].ToString()!,
                        CriadoEm = Convert.ToDateTime(reader["CriadoEm"]),
                        ExpiraEm = Convert.ToDateTime(reader["ExpiraEm"]),
                        RevogadoEm = reader["RevogadoEm"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(reader["RevogadoEm"]),
                        SubstituidoPorTokenHash = reader["SubstituidoPorTokenHash"] == DBNull.Value
                            ? null
                            : reader["SubstituidoPorTokenHash"].ToString()
                    };
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

            return refreshToken;
        }

        public void Revogar(long id, string? substituidoPorTokenHash)
        {
            SqlConnection con = BuscarConexao();

            try
            {
                AbrirConexao(con);

                string query = @"UPDATE PotyIA.RefreshTokens
                                 SET RevogadoEm = @RevogadoEm,
                                     SubstituidoPorTokenHash = @SubstituidoPorTokenHash
                                 WHERE Id = @Id AND RevogadoEm IS NULL";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@RevogadoEm", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@SubstituidoPorTokenHash",
                    (object?)substituidoPorTokenHash ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
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

        public void RevogarTodosDoUsuario(string usuarioID)
        {
            SqlConnection con = BuscarConexao();

            try
            {
                AbrirConexao(con);

                string query = @"UPDATE PotyIA.RefreshTokens
                                 SET RevogadoEm = @RevogadoEm
                                 WHERE UsuarioID = @UsuarioID AND RevogadoEm IS NULL";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@RevogadoEm", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                cmd.ExecuteNonQuery();
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
