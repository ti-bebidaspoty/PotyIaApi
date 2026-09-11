using PotyIaApi.Interfaces;
using PotyIaApi.Models;
using Microsoft.Data.SqlClient;

namespace PotyIaApi.Repositories
{
    public class UsuarioRepositorio : BasicoRepositorio, IUsuarioRepositorio
    {
        public UsuarioRepositorio(IConfiguration configuration) : base(configuration)
        {
        }

        public string CadastrarUsuario(UsuarioFormModel usuario)
        {
            SqlConnection con = BuscarConexao();

            try
            {
                AbrirConexao(con);

                string query = @"
            INSERT INTO Global.Usuarios
            (
                UsuarioID,
                Nome,
                Usuarios,
                Senha,
                DepartamentoID,
                Status,
                IsAdmin
            )
            OUTPUT INSERTED.UsuarioID
            VALUES
            (
                NEWID(),
                @Nome,
                @CPF,
                @Senha,
                NULL,
                1,
                0
            )";

                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Nome", usuario.Nome);
                    cmd.Parameters.AddWithValue("@CPF", usuario.CPF);
                    cmd.Parameters.AddWithValue("@Senha", usuario.Senha);

                    var resultado = cmd.ExecuteScalar();

                    if (resultado == null || resultado == DBNull.Value)
                    {
                        throw new Exception(
                            "Não foi possível obter o ID do usuário cadastrado."
                        );
                    }

                    return resultado.ToString()!;
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

        public UsuarioFormModel VerificarUsuarioSenior(string cpf)
        {
            SqlConnection con = BuscarConexaoSenior();
            UsuarioFormModel usuario = null!;

            try
            {
                AbrirConexao(con);

                string query = @"SELECT TOP 1
                                        NOMFUN AS Nome,
                                        NUMCPF AS CPF,
                                        REPLACE(CONVERT(VARCHAR(10), DATNAS, 103), '/', '') AS DataNascimento
                                    FROM VETORH_PROD.dbo.R034FUN
                                    WHERE NUMCPF = @CPF
                                        AND SITAFA <> 7
                                        AND DATAFA = CONVERT(DATETIME, '19001231', 112);";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CPF", cpf);
                using SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();

                if (reader.HasRows) usuario = new UsuarioFormModel
                {
                    Nome = reader["Nome"].ToString()!,
                    CPF = reader["CPF"].ToString()!,
                    Senha = reader["DataNascimento"].ToString()!,
                };
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                FecharConexao(con);
            }

            return usuario;
        }

        public bool VerificarUsuarioJaCadastrado(string cpf)
        {
            SqlConnection con = BuscarConexao();
            bool usuarioJaCadastrado = false;
            try
            {
                AbrirConexao(con);
                string query = @"SELECT COUNT(*) FROM Global.Usuarios WHERE Usuarios = @CPF";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CPF", cpf);
                    int count = (int)cmd.ExecuteScalar();
                    usuarioJaCadastrado = count > 0;
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
            return usuarioJaCadastrado;
        }

        public void CadastrarUsuarioAplicacao(string usuarioID)
        {
            SqlConnection con = BuscarConexao();
            try
            {
                AbrirConexao(con);
                string query = @"INSERT INTO Global.UsuariosAplicacoes (UsuarioID, AplicacaoID) VALUES (@UsuarioID, @AplicacaoID)";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);
                    cmd.Parameters.AddWithValue("@AplicacaoID", "531610b0-cf28-4c04-81bf-d2ea5b27949b");
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
