﻿using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using Presupuesto.Models;
using Presupuesto.Servicios;
using System.Data;
using System.Reflection;

namespace Presupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        public readonly IRepositorioServiciosUsuarios repositorioServiciosUsuarios;
        public readonly IRepositorioCuentas repositorioCuentas;
        public readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IMapper mapper;
        private readonly ISReportes sReportes;

        public TransaccionesController(IRepositorioServiciosUsuarios repositorioServiciosUsuarios,
                                        IRepositorioCuentas repositorioCuentas,
                                        IRepositorioCategorias repositorioCategorias,
                                        IRepositorioTransacciones repositorioTransacciones,
                                        IMapper mapper,
                                        ISReportes sReportes)
        {
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioServiciosUsuarios = repositorioServiciosUsuarios;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
            this.sReportes = sReportes;
        }

        public async Task<IActionResult> Index(int mes, int año)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var modelo = await sReportes.ObtenerRepTranDet(usuarioId, mes, año, ViewBag);
            return View(modelo);
        }


        public async Task<IActionResult> Crear()
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            modelo.UsuarioId = usuarioId;
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.Monto *= -1;
            }
            await repositorioTransacciones.Crear(modelo);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);
            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);

            modelo.MontoAnterior = modelo.Monto;

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }
            modelo.cuentaAnterioId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.UrlRetorno = urlRetorno;
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var trasaccion = mapper.Map<Transaccion>(modelo);
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                trasaccion.Monto *= -1;
            }
            await repositorioTransacciones.Actualizar(trasaccion, modelo.MontoAnterior, modelo.cuentaAnterioId);

            if (string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(modelo.UrlRetorno);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var transacion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);
            if (transacion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioTransacciones.Borrar(id);

            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.Obtener(usuarioId, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var categoria = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categoria);
        }

        public async Task<IActionResult> Semanal(int mes, int año)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana = await sReportes.ObtenerReporteSemanal
                (usuarioId, mes, año, ViewBag);

            //Agrupar semanas del mes
            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x => new ResultadoObtenerPorSemana()
            {
                Semana = x.Key,
                Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault(),
            }).ToList();

            if (año == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                año = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(año, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(año, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(año, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if (grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }
            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int año)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            if (año == 0)
            {
                año = DateTime.Today.Year;
            }
            var transaccionesPorMes = await repositorioTransacciones.ObtenerPorMes(usuarioId, año);
            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes).Select(x => new ResultadoObtenerPorMes()
            {
                Mes = x.Key,
                Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault(),
            }).ToList();

            for (int mes = 1; mes < 13; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(año, mes, 1);
                if (transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }
            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();
            modelo.Año = año;
            modelo.TransaccionesPorMes = transaccionesAgrupadas;

            return View(modelo);
        }

        public IActionResult ReporteExcel()
        {
            return View();
        }

        //Generar excel por reporte mensual
        [HttpGet]
        public async Task<FileResult> ExportarExcelPorMes(int mes, int año)
        {
            var fechaInicio = new DateTime(año, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });
            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        //Funcion para generar un archivo de excel desde un controlador (o se puede realizar un servicio)
        private FileResult GenerarExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                 new DataColumn("Fecha"),
                 new DataColumn("Cuenta"),
                 new DataColumn("Categoria"),
                 new DataColumn("Nota"),
                 new DataColumn("Monto"),
                 new DataColumn("Ingreso/Gasto"),
            });

            foreach (var i in transacciones)
            {
                dataTable.Rows.Add(i.FechaTransaccion, i.Cuenta, i.Categoria, i.Nota, i.Monto, i.TipoOperacionId);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        nombreArchivo);

                }
            }
        }

        //Generar excel por reporte anual
        [HttpGet]
        public async Task<FileResult> ExportarExcelPorAño(int año)
        {
            var fechaInicio = new DateTime(año, 1, 1);
            var fechaFin = fechaInicio.AddYears(1).AddDays(-1);
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });
            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        //Generar excel exportando todas las transacciones
        [HttpGet]
        public async Task<FileResult> ExportarExcelTodo(int año)
        {
            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(1000);
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });
            var nombreArchivo = $"Manejo Presupuesto - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = start,
                FechaFin = end
            });

            var eventosCalendario = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("N"),
                Start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = transaccion.FechaTransaccion.ToString("yyyy,MM,dd"),
                Color = (transaccion.TipoOperacionId == TipoOperacion.Gasto) ? "Red" : null
            });

            return Json(eventosCalendario);
        }

        //
        public async Task<JsonResult> ObetenerTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = fecha,
                FechaFin = fecha
            });

            return Json(transacciones);
        }
    }
}
