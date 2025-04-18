namespace Veterinaria_Waterhouse.Models
{
    public class TipoCambioViewModel
    {
        public double Compra { get; set; }
        public double Venta { get; set; }
        public string FechaActualizacion { get; set; }

    
        public string MonedaBase { get; set; }  // Para USD
        public string MonedaDestino { get; set; }  // Para CRC
    }
}
