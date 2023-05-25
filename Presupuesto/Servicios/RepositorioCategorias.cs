using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Presupuesto.Models;

namespace Presupuesto.Servicios
{
    public interface IRepositorioCategorias 
    {
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
    }
    public class RepositorioCategorias: IRepositorioCategorias
    {
        private readonly string ConnecctionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            ConnecctionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task Crear(Categoria categoria) 
        {
            using var coneccion = new SqlConnection(ConnecctionString);
            var id = await coneccion.QuerySingleAsync<int>(@"insert into Categorias (Nombre, TipoOperacionId, UsuarioId)
                                                            values (@Nombre, @TipoOperacionId, @UsuarioId)
                                                            select SCOPE_IDENTITY();", categoria);
            categoria.Id = id;
        }
        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId)
        {
            using var connnecion = new SqlConnection(ConnecctionString);
            return await connnecion.QueryAsync<Categoria>(@"select * from Categorias where UsuarioId = @UsuarioId", 
                                                            new { usuarioId });
        }
        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connnecion = new SqlConnection(ConnecctionString);
            return await connnecion.QueryAsync<Categoria>(@"select * from Categorias where UsuarioId = @UsuarioId AND
                                                           TipoOperacionId = @TipoOperacionId", 
                                                           new { usuarioId, tipoOperacionId });
        }
        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var coneccion = new SqlConnection(ConnecctionString);
            return await coneccion.QueryFirstOrDefaultAsync<Categoria>(@"select * from Categorias where id = @Id AND 
                                                                       UsuarioId = @UsuarioId", new { id, usuarioId });
        }
        public async Task Actualizar(Categoria categoria)
        {
            using var connecion = new SqlConnection(ConnecctionString);
            await connecion.ExecuteAsync(@"UPDATE Categorias SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionId
                                         where id = @id",categoria);
        }
        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(ConnecctionString);
            await connection.ExecuteAsync(@"Delete Categorias Where id = @id", new { id });
        }
    }
}
