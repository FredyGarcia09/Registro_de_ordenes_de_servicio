namespace Registro_de_ordenes_de_servicio.Backend.Models
{
    /// <summary>
    /// Estructura de transferencia de datos para la recepción de una orden desde el cliente
    /// </summary>
    public class OrdenDTO
    {
        public int IdVehiculo { get; set; }
        public decimal CostoTotal { get; set; }
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
}
