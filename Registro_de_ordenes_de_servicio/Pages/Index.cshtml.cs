using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registro_de_ordenes_de_servicio.Backend.DAOs;
using Registro_de_ordenes_de_servicio.Backend.Models;

namespace Registro_de_ordenes_de_servicio.Pages
{
    public class IndexModel : PageModel
    {

        private readonly CatalogosDAO _dao;

        /// <summary>
        /// Inicializa la página inyectando las dependencias de configuración.
        /// </summary>
        public IndexModel(IConfiguration configuration)
        {
            _dao = new CatalogosDAO(configuration);
        }

        public List<OrdenResumenDTO> ListaOrdenes { get; set; }

        public void OnGet()
        {
            ListaOrdenes = _dao.ObtenerHistorialOrdenes();
        }

        /// <summary>
        /// Endpoint asíncrono para recuperar los detalles de una orden específica.
        /// Se invoca mediante la ruta: ?handler=Detalles&folio=X
        /// </summary>
        public JsonResult OnGetDetalles(int folio)
        {
            var detalles = _dao.ObtenerDetallesPorFolio(folio);
            return new JsonResult(detalles);
        }
    }
}
