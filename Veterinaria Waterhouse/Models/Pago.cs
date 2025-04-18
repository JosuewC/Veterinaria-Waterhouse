namespace Veterinaria_Waterhouse.Models
{
    public class Pago
    {
        public string NumeroTarjeta { get; set; }
        public string Cvv { get; set; }
        public decimal MontoAPagar { get; set; }
        public string Nombre { get; set; }
        public string Identificacion { get; set; }
        public string Descripcion { get; set; }
    }
}
