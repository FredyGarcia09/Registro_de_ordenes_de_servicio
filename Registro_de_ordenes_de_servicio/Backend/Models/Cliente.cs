namespace Registro_de_ordenes_de_servicio.Backend.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Rfc { get; set; }
        public string NombreCompleto { get; set; } // Nombre + Apellidos
    }
}
