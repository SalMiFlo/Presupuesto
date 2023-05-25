using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Presupuesto.Models;
using Presupuesto.Servicios;

namespace Presupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IRepositorioServiciosUsuarios repositorioServiciosUsuarios;
        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IRepositorioServiciosUsuarios
            repositorioServiciosUsuarios)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.repositorioServiciosUsuarios = repositorioServiciosUsuarios;
        }
        public IActionResult Crear()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }
            tipoCuenta.UsuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();

            var yaExiste = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre!, tipoCuenta.UsuarioId);
            if (yaExiste)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe!");
                return View(tipoCuenta);
            }

            await repositorioTiposCuentas.Crear(tipoCuenta);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var tipoCuentaExiste = await repositorioTiposCuentas.ObtenerPorId(tipoCuenta.Id, usuarioId);
            if (tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarPost(int id)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioTiposCuentas.Borrar(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Verificar(string nombre)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var yaExiste = await repositorioTiposCuentas.Existe(nombre, usuarioId);
            if (yaExiste)
            {
                return Json($"El nombre {nombre} ya existe!");
            }
            return Json(true);
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return View(tiposCuentas);
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = repositorioServiciosUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);
            var idsTiposCuentasNoPertenecen = ids.Except(idsTiposCuentas).ToList();
            if (idsTiposCuentasNoPertenecen.Count > 0) 
            {
                return Forbid();
            }
            var tiposCuentasOrdenados = ids.Select((valor, indice) => 
                                        new TipoCuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();
            await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);
            return Ok();
        }
    }
}
