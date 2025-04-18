namespace Veterinaria_Waterhouse.Models
{
    public class Medicina
    {
        public int Id { get; set; }
        public string NombreMedicina { get; set; }
        public int Unidades { get; set; }
        public decimal ValorUnitario { get; set; }
        public string Detalle { get; set; }
    }
}
