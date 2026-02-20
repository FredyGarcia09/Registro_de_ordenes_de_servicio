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

        /// <summary>
        /// Recupera la lista de vehículos que pertenecen a un cliente específico.
        /// </summary>
        /// <param name="idCliente">Identificador único del cliente.</param>
        /// <returns>Colección de vehículos asociados.</returns>
        public List<Vehiculo> ObtenerVehiculosPorCliente(int idCliente)
        {
            var lista = new List<Vehiculo>();

            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                string query = "SELECT idVehiculo, placas, marca, modelo FROM vehiculos WHERE idCliente = @idCliente";
                using (var cmd = new SqlCommand(query, conexion))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@idCliente", idCliente);

                    conexion.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Vehiculo
                            {
                                IdVehiculo = dr.GetInt32(dr.GetOrdinal("idVehiculo")),
                                Placas = dr.GetString(dr.GetOrdinal("placas")),
                                Marca = dr.GetString(dr.GetOrdinal("marca")),
                                Modelo = dr.GetString(dr.GetOrdinal("modelo"))
                            });
                        }
                    }
                }
            }
            return lista;
        }


        /// <summary>
        /// Obtiene el siguiente folio estimado para una nueva orden de servicio.
        /// Consulta el último valor de identidad generado en la tabla y le suma uno.
        /// </summary>
        /// <returns>El número del próximo folio a generar</returns>
        public int ObtenerSiguienteFolio()
        {
            int siguienteFolio = 1;

            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                // IDENT_CURRENT recupera el último identity.
                // ISNULL previene si la tabla está vacía.
                string query = "SELECT ISNULL(IDENT_CURRENT('ordenesServicio'), 0) + 1";

                using (var cmd = new SqlCommand(query, conexion))
                {
                    conexion.Open();
                    // ExecuteScalar ejecuta la consulta y devuelve la primera.
                    siguienteFolio = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return siguienteFolio;
        }


        /// <summary>
        /// Inserta una nueva orden de servicio y sus detalles utilizando una transacción.
        /// </summary>
        /// <param name="orden">Objeto DTO que contiene la cabecera y el arreglo de servicios.</param>
        /// <returns>El folio generado por la base de datos, o 0 si ocurre un error.</returns>
        public int GuardarOrdenTransaccional(OrdenDTO orden)
        {
            int folioGenerado = 0;

            using (var conexion = new SqlConnection(_cadenaConexion))
            {
                conexion.Open();

                // Inicia transacción
                using (var transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        //Inserción de la tabla ordenesServicio
                        string queryMaestro = @"
                            INSERT INTO ordenesServicio (idVehiculo, fechaIngreso, estado, costoTotal) 
                            VALUES (@idVehiculo, GETDATE(), 'Abierta', @costoTotal);
                            SELECT SCOPE_IDENTITY();";

                        using (var cmdMaestro = new SqlCommand(queryMaestro, conexion, transaccion))
                        {
                            cmdMaestro.Parameters.AddWithValue("@idVehiculo", orden.IdVehiculo);
                            cmdMaestro.Parameters.AddWithValue("@costoTotal", orden.CostoTotal);

                            folioGenerado = Convert.ToInt32(cmdMaestro.ExecuteScalar());
                        }

                        // Inserción de la tabla ordenDetallesServicios
                        string queryDetalle = @"
                            INSERT INTO ordenDetallesServicios (folioOrden, claveServicio, precioAlMomento) 
                            VALUES (@folioOrden, @claveServicio, @precioAlMomento);";

                        using (var cmdDetalle = new SqlCommand(queryDetalle, conexion, transaccion))
                        {
                            // Prepara los parametros para reutilizarlos en el ciclo
                            cmdDetalle.Parameters.Add("@folioOrden", SqlDbType.Int);
                            cmdDetalle.Parameters.Add("@claveServicio", SqlDbType.VarChar, 20);
                            cmdDetalle.Parameters.Add("@precioAlMomento", SqlDbType.Decimal);

                            // Itera sobre el arreglo y ejecuta el query por cada elemento
                            foreach (var detalle in orden.Detalles)
                            {
                                cmdDetalle.Parameters["@folioOrden"].Value = folioGenerado;
                                cmdDetalle.Parameters["@claveServicio"].Value = detalle.ClaveServicio;
                                cmdDetalle.Parameters["@precioAlMomento"].Value = detalle.PrecioAlMomento;

                                cmdDetalle.ExecuteNonQuery();
                            }
                        }

                        // Confirma los cambios
                        transaccion.Commit();
                    }
                    catch (Exception)
                    {
                        // En caso de error
                        transaccion.Rollback();
                        throw;
                    }
                }
            }

            return folioGenerado;
        }
    }
}
