using Microsoft.Data.SqlClient;
using PotyIaApi.Interfaces;
using PotyIaApi.Models;

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

                const string query = @"INSERT INTO Global.RefreshTokens
                                           (TokenHash, UsuarioID, CriadoEm, ExpiraEm)
                                       VALUES
                                           (@TokenHash, @UsuarioID, @CriadoEm, @ExpiraEm);";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@TokenHash", System.Data.SqlDbType.VarChar, 128).Value = refreshToken.TokenHash;
                cmd.Parameters.Add("@UsuarioID", System.Data.SqlDbType.VarChar, 50).Value = refreshToken.UsuarioID;
                cmd.Parameters.Add("@CriadoEm", System.Data.SqlDbType.DateTime2).Value = refreshToken.CriadoEm;
                cmd.Parameters.Add("@ExpiraEm", System.Data.SqlDbType.DateTime2).Value = refreshToken.ExpiraEm;

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

                const string query = @"SELECT Id, TokenHash, UsuarioID, CriadoEm, ExpiraEm,
                                              RevogadoEm, SubstituidoPorTokenHash
                                       FROM Global.RefreshTokens
                                       WHERE TokenHash = @TokenHash;";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@TokenHash", System.Data.SqlDbType.VarChar, 128).Value = tokenHash;

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
                        RevogadoEm = reader["RevogadoEm"] == DBNull.Value ? null : Convert.ToDateTime(reader["RevogadoEm"]),
                        SubstituidoPorTokenHash = reader["SubstituidoPorTokenHash"] == DBNull.Value ? null : reader["SubstituidoPorTokenHash"].ToString()
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

        public void Revogar(long id, string substituidoPorTokenHash)
        {
            SqlConnection con = BuscarConexao();

            try
            {
                AbrirConexao(con);

                const string query = @"UPDATE Global.RefreshTokens
                                       SET RevogadoEm = @RevogadoEm,
                                           SubstituidoPorTokenHash = @SubstituidoPorTokenHash
                                       WHERE Id = @Id
                                         AND RevogadoEm IS NULL;";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@RevogadoEm", System.Data.SqlDbType.DateTime2).Value = DateTime.UtcNow;
                cmd.Parameters.Add("@SubstituidoPorTokenHash", System.Data.SqlDbType.VarChar, 128).Value = substituidoPorTokenHash;
                cmd.Parameters.Add("@Id", System.Data.SqlDbType.BigInt).Value = id;

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

                const string query = @"UPDATE Global.RefreshTokens
                                       SET RevogadoEm = @RevogadoEm
                                       WHERE UsuarioID = @UsuarioID
                                         AND RevogadoEm IS NULL;";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@RevogadoEm", System.Data.SqlDbType.DateTime2).Value = DateTime.UtcNow;
                cmd.Parameters.Add("@UsuarioID", System.Data.SqlDbType.VarChar, 50).Value = usuarioID;

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
