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

        public void OnGet()
        {
            // Al cargar la página, llenamos listas desde DB
            ListaClientes = _dao.ObtenerClientes();
            ListaServicios = _dao.ObtenerServicios();
        }
    }
}
