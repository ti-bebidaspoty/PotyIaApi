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

        public void CadastrarUsuario(UsuarioFormModel usuario)
        {
            SqlConnection con = BuscarConexao();

            try
            {
                AbrirConexao(con);
                string query = @"INSERT INTO Global.Usuarios(UsuarioID, Nome, Usuarios, Senha, DepartamentoID, Status, IsAdmin) VALUES (NEWID(), @Nome, @CPF, @Senha, null, 1, 0)";

                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Nome", usuario.Nome);
                    cmd.Parameters.AddWithValue("@CPF", usuario.CPF);
                    cmd.Parameters.AddWithValue("@Senha", usuario.Senha);

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
    }
}
