using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositoriocuentas;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositoriocuentas)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
            this.repositoriocuentas = repositoriocuentas;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuentasConTipoCuenta = await repositoriocuentas.Buscar(usuarioId);

            var modelo = cuentasConTipoCuenta
                .GroupBy(x => x.TipoCuenta)
                .Select(grupo => new IndiceCuentasViewModel
                {
                    TipoCuenta = grupo.Key,
                    Cuentas = grupo.AsEnumerable()
                }).ToList();

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuenta = await repositorioTiposCuentas.Obtener(usuarioId);
            var modelo = new CuentaCreacionViewModel();

            modelo.TiposCuenta = await ObtenerTiposCuentas(usuarioId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuenta = await repositorioTiposCuentas.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

            if (tiposCuenta == null)
            {
                return RedirectToAction("No Encontrado", "Home");
            }

            if (!ModelState.IsValid)
            {
                cuenta.TiposCuenta = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);

            }

            await repositoriocuentas.Crear(cuenta);
            return RedirectToAction("Index");

        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuenta = await repositorioTiposCuentas.Obtener(usuarioId);
            return tiposCuenta.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}
