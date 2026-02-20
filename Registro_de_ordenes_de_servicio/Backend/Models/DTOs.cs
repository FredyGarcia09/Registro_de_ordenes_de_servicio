namespace Registro_de_ordenes_de_servicio.Backend.Models
{
    /// <summary>
    /// Estructura de transferencia de datos para la recepción de una orden desde el cliente
    /// </summary>
    public class OrdenDTO
    {
        public int IdVehiculo { get; set; }
        public decimal CostoTotal { get; set; }
        public DateTime? FechaEstimadaEntrega { get; set; }
        public List<DetalleOrdenDTO> Detalles { get; set; }
    }

    /// <summary>
    /// Representa un renglón individual dentro del arreglo de servicios de la orden
    /// </summary>
    public class DetalleOrdenDTO
    {
        public string ClaveServicio { get; set; }
        public decimal PrecioAlMomento { get; set; }
    }

    /// <summary>
    /// Estructura de transferencia de datos para historial de órdenes.
    /// </summary>
    public class OrdenResumenDTO
    {
        public int Folio { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreCliente { get; set; }
        public string InfoVehiculo { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
    }


    /// <summary>
    /// Transferencia de datos para visualización de los renglones que componen una orden.
    /// </summary>
    public class DetalleResumenDTO
    {
        public string NombreServicio { get; set; }
        public decimal PrecioCobrado { get; set; }
    }
}
