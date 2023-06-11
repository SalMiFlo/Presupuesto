using Dapper;
using Microsoft.Data.SqlClient;
using Presupuesto.Models;

namespace Presupuesto.Servicios
{
    public interface IRepositorioUsuarios
    {
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task<int> CrearUsuario(Usuario usuario);
    }
    public class RepositorioUsuarios: IRepositorioUsuarios
    {
        private readonly string ConnectionString; 
        public RepositorioUsuarios(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var connection = new SqlConnection(ConnectionString);
            var id = await connection.QuerySingleAsync<int>(@"insert into Usuarios (Email, EmailNormalizado, PasswordHash) 
                                                            values (@Email, @EmailNormalizado, @PasswordHash); select 
                                                            SCOPE_IDENTITY();", usuario);
            return id;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(ConnectionString);
            return await connection.QuerySingleOrDefaultAsync<Usuario>(@"select * from Usuarios where EmailNormalizado = 
                                                                       @EmailNormalizado", new { emailNormalizado });
        }
    }
}
