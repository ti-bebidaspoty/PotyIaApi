using PotyIaApi.Interfaces;
using PotyIaApi.Models;
using Microsoft.Data.SqlClient;

namespace PotyIaApi.Repositories
{
	public class AutenticacaoRepositorio : BasicoRepositorio, IAutenticacaoRepositorio
	{
		private readonly IConfiguration _configuracao;

		public AutenticacaoRepositorio(IConfiguration configuration) : base(configuration)
		{
			_configuracao = configuration;
		}

		public async Task<UsuarioInternoModel?> BuscarUsuario(string usuario)
		{
			var aplicacaoId = _configuracao["Aplicacoes:PotyIA"];

			if (string.IsNullOrWhiteSpace(aplicacaoId))
				throw new InvalidOperationException(
					"A configuração 'Aplicacoes:PotyIA' não foi encontrada no appsettings.json.");

			SqlConnection con = BuscarConexao();
			UsuarioInternoModel? usuarioInterno = null;

			try
			{
				AbrirConexao(con);

				// A senha NÃO é comparada no SQL. Apenas localizamos o usuário
				// ativo, vinculado à aplicação PotyIA e com a aplicação ativa.
				const string query = @"SELECT
											U.UsuarioID,
											U.Nome,
											U.Usuarios,
											U.Senha,
											U.Status
										FROM Global.Usuarios U
										INNER JOIN Global.UsuariosAplicacoes UA
											ON UA.UsuarioID = U.UsuarioID
										INNER JOIN Global.Aplicacoes A
											ON A.AplicacaoID = UA.AplicacaoID
										WHERE U.Usuarios = @Usuario
										  AND U.Status = 1
										  AND UA.AplicacaoID = @AplicacaoID
										  AND A.Status = 1;";

				using var cmd = new SqlCommand(query, con);
				cmd.Parameters.Add("@Usuario", System.Data.SqlDbType.VarChar, 200).Value = usuario;
				cmd.Parameters.Add("@AplicacaoID", System.Data.SqlDbType.VarChar, 50).Value = aplicacaoId;

				using SqlDataReader reader = await cmd.ExecuteReaderAsync();

				if (await reader.ReadAsync())
				{
					usuarioInterno = new UsuarioInternoModel
					{
						UsuarioID = reader["UsuarioID"].ToString()!,
						Nome = reader["Nome"].ToString()!,
						Usuario = reader["Usuarios"].ToString()!,
						SenhaHash = reader["Senha"].ToString()!,
						Status = Convert.ToBoolean(reader["Status"])
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

			return usuarioInterno;
		}

        public async Task<bool> UsuarioExiste(string usuario)
        {
            SqlConnection con = BuscarConexao();

            try
            {
                AbrirConexao(con);

                const string query = @"
            SELECT COUNT(1)
            FROM Global.Usuarios
            WHERE Usuarios = @Usuario;";

                using var cmd = new SqlCommand(query, con);

                cmd.Parameters.Add(
                    "@Usuario",
                    System.Data.SqlDbType.VarChar,
                    200
                ).Value = usuario;

                var quantidade = Convert.ToInt32(
                    await cmd.ExecuteScalarAsync()
                );

                return quantidade > 0;
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
