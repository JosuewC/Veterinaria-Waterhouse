namespace Veterinaria_Waterhouse.Models
{
    
public class Evento
{
    public int Id { get; set; }

    // Nombre del usuario afectado
    public string Nombre { get; set; }

    // Identificación del usuario
    public string Identificación { get; set; }

    // Acción realizada por el usuario
    public string Accion { get; set; }

    // Fecha y hora del evento
    public DateTime Fecha { get; set; } = DateTime.Now;
}
}
