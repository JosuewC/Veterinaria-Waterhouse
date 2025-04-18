namespace Veterinaria_Waterhouse.Models
{
    public class Ubicacion
    {

        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }  // "pais", "provincia", "canton", "distrito"
        public int? IdPadre { get; set; } // Relación jerárquica
    }
}
