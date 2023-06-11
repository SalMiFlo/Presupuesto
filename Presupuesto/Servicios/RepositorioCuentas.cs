using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Presupuesto.Models;
using System.Collections.Generic;

namespace Presupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Crear(Cuenta cuenta);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
    }
    public class RepositorioCuentas: IRepositorioCuentas
    {
        private readonly string ConnecctionString;
        public RepositorioCuentas(IConfiguration configuration)
        {
            ConnecctionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var conection = new SqlConnection(ConnecctionString);
            var id = await conection.QuerySingleAsync<int>(@"INSERT INTO Cuentas (Nombre, TipoCuentaId, Descripcion, Balance) 
                                                            VALUES (@Nombre, @TipoCuentaId, @Descripcion, @Balance);
                                                            SELECT SCOPE_IDENTITY();", cuenta);
            cuenta.Id = id;
        }
        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId) 
        {
            using var conection = new SqlConnection(ConnecctionString);
            return await conection.QueryAsync<Cuenta>(@"SELECT C.id, C.Nombre, C.Balance, TC.Nombre AS TipoCuenta 
                                                        FROM Cuentas C INNER JOIN TiposCuentas TC ON TC.id = C.TipoCuentaId 
                                                        WHERE TC.UsuarioId = @UsuarioId ORDER BY TC.Orden", new { usuarioId });
        }
        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connecion = new SqlConnection(ConnecctionString);
            return await connecion.QueryFirstOrDefaultAsync<Cuenta>(@"SELECT C.id, C.Nombre, C.Balance, C.Descripcion, TipoCuentaId  
                                                        FROM Cuentas C INNER JOIN TiposCuentas TC ON TC.id = C.TipoCuentaId 
                                                        WHERE TC.UsuarioId = @UsuarioId AND C.id = @Id", new { id, usuarioId });
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connecion = new SqlConnection(ConnecctionString);
            await connecion.ExecuteAsync(@"UPDATE Cuentas SET Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion, 
                                          TipoCuentaId = @TipoCuentaId Where id = @id", cuenta);
        }

        public async Task Borrar(int id) 
        {
            using var connecion = new SqlConnection(ConnecctionString);
            await connecion.ExecuteAsync(@"DELETE Cuentas WHERE id = @Id", new { id });
        }
    }
}
