namespace Veterinaria_Waterhouse.Models
{
    public class Cliente
    {
      
            public string Nombre { get; set; }  
            public string Apellidos { get; set; } 
            public string Identificacion { get; set; }  
            public string Celular { get; set; } 
            public string Correo { get; set; }  
            public string Usuario { get; set; } 
            public string Contrasena { get; set; }
        public string Contraseña { get; internal set; }
    }
}
