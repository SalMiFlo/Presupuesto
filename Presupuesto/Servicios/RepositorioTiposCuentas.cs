using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Presupuesto.Controllers;
using Presupuesto.Models;

namespace Presupuesto.Servicios
{
    public interface IRepositorioTiposCuentas 
    {
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task Actualizar(TipoCuenta tipoCuenta);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
        Task Borrar(int id);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
    }
    public class RepositorioTiposCuentas: IRepositorioTiposCuentas
    {
        private readonly string ConnecctionString;
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            ConnecctionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var conection = new SqlConnection(ConnecctionString);
            var id = await conection.QuerySingleAsync<int>($@"TiposCuentasInsertar", new { usuarioId = tipoCuenta.UsuarioId,
                                                          nombre = tipoCuenta.Nombre}, commandType: 
                                                          System.Data.CommandType.StoredProcedure);
            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connecion = new SqlConnection(ConnecctionString);
            var existe = await connecion.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM TiposCuentas 
                                                                        WHERE Nombre = @Nombre and UsuarioId = @UsuarioId;", 
                                                                        new { nombre, usuarioId });
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connecion = new SqlConnection(ConnecctionString);
            return await connecion.QueryAsync<TipoCuenta>("select id, Nombre, Orden from TiposCuentas where " +
                                                            "UsuarioId = @usuarioId ORDER BY Orden;", new { usuarioId }); 
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connecion = new SqlConnection(ConnecctionString);
            await connecion.ExecuteAsync(@"UPDATE TiposCuentas SET Nombre = @Nombre WHERE id = @id", 
                                          tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connecion = new SqlConnection(ConnecctionString);
            return await connecion.QueryFirstOrDefaultAsync<TipoCuenta>(@"select id, Nombre, Orden from TiposCuentas where id = @id 
                                                                        and UsuarioId = @usuarioId;", new { id, usuarioId });
        }
        public async Task Borrar(int id)
        {
            using var connecion = new SqlConnection(ConnecctionString);
            await connecion.ExecuteAsync(@"delete TiposCuentas where id = @id", new { id });
        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados) 
        {
            var query = "update TiposCuentas set Orden = @Orden where id = @id;";
            using var connecion = new SqlConnection(ConnecctionString);
            await connecion.ExecuteAsync(query, tipoCuentasOrdenados);
        }
    }
}
