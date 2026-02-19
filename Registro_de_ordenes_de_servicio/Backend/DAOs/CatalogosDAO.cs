using Registro_de_ordenes_de_servicio.Backend.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Registro_de_ordenes_de_servicio.Backend.DAOs
{
    /// <summary>
    /// DAO encargado de gestionar la recuperación de catálogos generales (Clientes, Servicios) desde la base de datos.
    /// </summary>
    public class CatalogosDAO
    {
        private readonly string _cadenaConexion;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="CatalogosDAO"/> inyectando la configuración.
        /// </summary>
        /// <param name="configuration">Interfaz para acceder a appsettings.json.</param>
        public CatalogosDAO(IConfiguration configuration)
        {
            _cadenaConexion = configuration.GetConnectionString("CadenaSQL")
                ?? throw new ArgumentNullException("La cadena de conexión 'CadenaSQL' no está definida.");
        }

        /// <summary>
        /// Recupera el catálogo completo de clientes registrados.
        /// </summary>
        /// <returns>Una colección de objetos <see cref="Cliente"/>.</returns>
        public List<Cliente> ObtenerClientes()
        {
            var lista = new List<Cliente>();

            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                string query = "SELECT idCliente, rfc, (nombre + ' ' + apellidoPaterno) AS NombreCompleto FROM clientes";
                using (var cmd = new SqlCommand(query, conexion))
                {
                    cmd.CommandType = CommandType.Text;
                    conexion.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Cliente
                            {
                                IdCliente = dr.GetInt32(dr.GetOrdinal("idCliente")),
                                Rfc = dr.GetString(dr.GetOrdinal("rfc")),
                                NombreCompleto = dr.GetString(dr.GetOrdinal("NombreCompleto"))
                            });
                        }
                    }
                }
            }
            return lista;
        }

        /// <summary>
        /// Recupera el catálogo de servicios de mano de obra disponibles.
        /// </summary>
        /// <returns>Una colección de objetos <see cref="Servicio"/>.</returns>
        public List<Servicio> ObtenerServicios()
        {
            var lista = new List<Servicio>();

            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                string query = "SELECT claveServicio, nombreServicio, costoBase FROM servicios";
                using (var cmd = new SqlCommand(query, conexion))
                {
                    cmd.CommandType = CommandType.Text;
                    conexion.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Servicio
                            {
                                ClaveServicio = dr.GetString(dr.GetOrdinal("claveServicio")),
                                NombreServicio = dr.GetString(dr.GetOrdinal("nombreServicio")),
                                CostoBase = dr.GetDecimal(dr.GetOrdinal("costoBase"))
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}
