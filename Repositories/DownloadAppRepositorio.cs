using Microsoft.Data.SqlClient;
using PotyIaApi.Interfaces;

namespace PotyIaApi.Repositories
{
    public class DownloadAppRepositorio : BasicoRepositorio, IDownloadAppRepositorio
    {
        public DownloadAppRepositorio(IConfiguration configuration)
            : base(configuration)
        {
        }

        public string DownloadAppApple()
        {
            SqlConnection con = BuscarConexao();

            try
            {
                AbrirConexao(con);

                string query = @"
                    ;WITH ProximoLink AS
                    (
                        SELECT TOP (1)
                            ID,
                            Link,
                            Usado
                        FROM PotyIA.LinksApple WITH (UPDLOCK, READPAST, ROWLOCK)
                        WHERE Usado = 0
                        ORDER BY ID
                    )
                    UPDATE ProximoLink
                    SET Usado = 1
                    OUTPUT INSERTED.Link;
                ";

                using (var cmd = new SqlCommand(query, con))
                {
                    var result = cmd.ExecuteScalar();

                    return result != null
                        ? result.ToString()!
                        : string.Empty;
                }
            }
            finally
            {
                FecharConexao(con);
            }
        }
    }
}