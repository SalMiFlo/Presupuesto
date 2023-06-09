﻿using Dapper;
using Microsoft.Data.SqlClient;
using Presupuesto.Models;

namespace Presupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Crear(Transaccion transaccion);
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task Borrar(int id);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año);
    }
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string ConnecctionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            ConnecctionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var coneccion = new SqlConnection(ConnecctionString);
            var id = await coneccion.QuerySingleAsync<int>("Transacciones_Insertar", new
            {
                transaccion.UsuarioId,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                transaccion.CategoriaId,
                transaccion.CuentaId,
                transaccion.Nota
            }, commandType: System.Data.CommandType.StoredProcedure);
            transaccion.Id = id;
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var coneccion = new SqlConnection(ConnecctionString);
            await coneccion.ExecuteAsync("Transacciones_Actualizar", new
            {
                transaccion.Id,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                transaccion.CategoriaId,
                transaccion.CuentaId,
                transaccion.Nota,
                montoAnterior,
                cuentaAnteriorId
            }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var coneccion = new SqlConnection(ConnecctionString);
            return await coneccion.QueryFirstOrDefaultAsync<Transaccion>(@"select Ta.*, Ca.TipoOperacionId from Transacciones Ta 
                                                                     Inner Join Categorias Ca ON ca.id = Ta.CategoriaId 
                                                                     Where Ta.id = @id AND Ta.UsuarioID = @UsuarioId",
                                                                     new { id, usuarioId });
        }

        public async Task Borrar(int id)
        {
            using var coneccion = new SqlConnection(ConnecctionString);
            await coneccion.ExecuteAsync("Transacciones_Borrar", new { id },
                                         commandType: System.Data.CommandType.StoredProcedure);
        }

        //Para manejo de reportes sobre nuestras cuentas
        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var coneccion = new SqlConnection(ConnecctionString);
            return await coneccion.QueryAsync<Transaccion>(@"select Ta.id, Ta.monto, Ta.FechaTransaccion, 
                                                           Ca.Nombre as Categoria, Cu.Nombre as Cuenta, 
                                                           Ca.TipoOperacionId from Transacciones Ta inner join Categorias Ca
                                                           on Ca.id = Ta.CategoriaId inner join Cuentas Cu on 
                                                           Cu.id = Ta.CuentaId where Ta.CuentaId = @CuentaId AND 
                                                           Ta.UsuarioID = @UsuarioId AND Ta.FechaTransaccion between 
                                                           @FechaInicio AND @FechaFin", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var coneccion = new SqlConnection(ConnecctionString);
            return await coneccion.QueryAsync<Transaccion>(@"select Ta.id, Ta.monto, Ta.FechaTransaccion, 
                                                           Ca.Nombre as Categoria, Cu.Nombre as Cuenta, 
                                                           Ca.TipoOperacionId, Nota from Transacciones Ta inner join Categorias Ca
                                                           on Ca.id = Ta.CategoriaId inner join Cuentas Cu on 
                                                           Cu.id = Ta.CuentaId where Ta.UsuarioID = @UsuarioId AND 
                                                           Ta.FechaTransaccion between @FechaInicio AND @FechaFin
                                                           Order by Ta.FechaTransaccion DESC", modelo);
        }

        //Para reportes semanales
        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var coneccion = new SqlConnection(ConnecctionString);
            return await coneccion.QueryAsync<ResultadoObtenerPorSemana>(@"select datediff(d, @fechaInicio, FechaTransaccion)/7 + 1
                                                                         as Semana, sum(monto) as Monto, Ca.TipoOperacionId
                                                                         from Transacciones Tra inner join Categorias Ca on 
                                                                         Ca.id = Tra.CategoriaId where Tra.UsuarioID = @usuarioId 
                                                                         and FechaTransaccion between @fechaInicio and @fechaFin
                                                                         group by datediff(d, @fechaInicio, FechaTransaccion)/7, 
                                                                         Ca.TipoOperacionId", modelo);
        }

        //Para reportes mensuales
        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año)
        {
            using var connection = new SqlConnection(ConnecctionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"select MONTH(FechaTransaccion) as Mes, SUM(monto) as Monto,
                                                                       Ca.TipoOperacionId from Transacciones Ta inner join 
                                                                       Categorias Ca on Ca.id = Ta.CategoriaId where 
                                                                       Ta.UsuarioID = @usuarioId and YEAR(FechaTransaccion) = @Año
                                                                       group by MONTH(FechaTransaccion), Ca.TipoOperacionId", 
                                                                       new { usuarioId, año });
        }
    }
}
