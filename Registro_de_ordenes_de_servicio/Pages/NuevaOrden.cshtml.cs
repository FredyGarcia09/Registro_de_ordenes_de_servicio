using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Registro_de_ordenes_de_servicio.Backend.DAOs;
using Registro_de_ordenes_de_servicio.Backend.Models;

namespace Registro_de_ordenes_de_servicio.Pages
{
    public class NuevaOrdenModel : PageModel
    {
        private readonly CatalogosDAO _dao;

        public NuevaOrdenModel(IConfiguration configuration)
        {
            _dao = new CatalogosDAO(configuration);
        }

        // Listas de datos para enviarlos al HTML
        public List<Cliente> ListaClientes { get; set; }
        public List<Servicio> ListaServicios { get; set; }
        public int FolioEstimado { get; set; }

        public void OnGet()
        {
            // Al cargar la página, llenamos listas desde DB
            ListaClientes = _dao.ObtenerClientes();
            ListaServicios = _dao.ObtenerServicios();
            FolioEstimado = _dao.ObtenerSiguienteFolio();
        }

        /// <summary>
        /// Endpoint asíncrono que responde a peticiones AJAX para cargar vehículos.
        /// Se invoca usando la ruta: ?handler=Vehiculos&idCliente=X
        /// </summary>
        public JsonResult OnGetVehiculos(int idCliente)
        {
            var vehiculos = _dao.ObtenerVehiculosPorCliente(idCliente);
            return new JsonResult(vehiculos);
        }


        /// <summary>
        /// Endpoint asíncrono que recibe el DTO desde el cliente y coordina el guardado.
        /// Se invoca mediante POST a la ruta ?handler=Guardar
        /// </summary>
        /// <param name="ordenData">El payload JSON deserializado automáticamente por el framework.</param>
        public JsonResult OnPostGuardar([FromBody] OrdenDTO ordenData)
        {
            try
            {
                // Validación básica
                if (ordenData == null || ordenData.Detalles == null || !ordenData.Detalles.Any())
                {
                    return new JsonResult(new { exito = false, mensaje = "Estructura de datos inválida o vacía." });
                }

                // Invoca la capa de acceso a datos
                int nuevoFolio = _dao.GuardarOrdenTransaccional(ordenData);

                if (nuevoFolio > 0)
                {
                    return new JsonResult(new { exito = true, folio = nuevoFolio });
                }
                else
                {
                    return new JsonResult(new { exito = false, mensaje = "No se pudo generar el folio en la base de datos." });
                }
            }
            catch (Exception ex)
            {
                // Depuración
                return new JsonResult(new { exito = false, mensaje = ex.Message });
            }
        }
    }
}
