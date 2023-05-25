using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using Presupuesto.Models;
using Presupuesto.Servicios;
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

        public TransaccionesController(IRepositorioServiciosUsuarios repositorioServiciosUsuarios,
                                        IRepositorioCuentas repositorioCuentas,
                                        IRepositorioCategorias repositorioCategorias,
                                        IRepositorioTransacciones repositorioTransacciones,
                                        IMapper mapper)
        {
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioServiciosUsuarios = repositorioServiciosUsuarios;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index(int mes, int año) 
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();

            DateTime fechaInicio;
            DateTime fechaFin;
            if (mes <= 0 || mes > 12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(año, mes, 1);
            }
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            var parametro = new ParametroObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(parametro);

            var modelo = new ReporteTransaccionesDetalladas();

            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                                        .GroupBy(x => x.FechaTransaccion)
                                        .Select(grupo => new ReporteTransaccionesDetalladas.TransaccionesPorFecha()
                                        {
                                            FechaTransaccion = grupo.Key,
                                            Transacciones = grupo.AsEnumerable()
                                        });

            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.añoAnterior = fechaInicio.AddMonths(-1).Year;
            ViewBag.mesPosterior = fechaFin.AddMonths(1).Month;
            ViewBag.añoPosterior = fechaFin.AddMonths(1).Year;
            ViewBag.urlRetorno = HttpContext.Request.Path + HttpContext.Request.QueryString;

            return View(modelo);
            return View();

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
    }
}
