using PotyIaApi.Helpers;
using PotyIaApi.Interfaces;
using PotyIaApi.Models;
using System.Data.SqlClient;

namespace PotyIaApi.Repositories
{
    public class AutenticacaoRepositorio : BasicoRepositorio, IAutenticacaoRepositorio
    {
        private readonly IHelper _helper;

        public AutenticacaoRepositorio(IConfiguration configuration, IHelper helper) : base(configuration)
        {
            _helper = helper;
        }

        public UsuarioAutenticadoModel RealizarAutenticacao(AutenticacaoModel autenticacao)
        {
            SqlConnection con = BuscarConexao();
            UsuarioAutenticadoModel usuarioLogado = null!;

            try
            {
                AbrirConexao(con);

                string query = @"SELECT UsuarioID, Nome
	                                FROM PotyIA.Usuarios
	                                WHERE CPF = @CPF AND Senha = @Senha AND Ativo = 1";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CPF", _helper.Criptografar(autenticacao.CPF));
                cmd.Parameters.AddWithValue("@Senha", _helper.Criptografar(autenticacao.Senha));
                using SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();

                if (reader.HasRows) usuarioLogado = new UsuarioAutenticadoModel
                {
                    UsuarioID = reader["UsuarioID"].ToString()!,
                    Nome = reader["Nome"].ToString()!,
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

            return usuarioLogado;
        }
    }
}
