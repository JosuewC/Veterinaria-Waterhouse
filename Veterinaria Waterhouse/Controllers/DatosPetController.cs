using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using Veterinaria_Waterhouse.Models;

namespace Veterinaria_Waterhouse.Controllers
{
    public class DatosPetController : Controller
    {
        private readonly string _connectionString = "Server=localhost;Database=veterinaria;User=root;Password=Josue23+WC;";

        // Acción para visualizar los datos de la mascota
        public IActionResult DatosPetvista(string nombre)
        {
            Mascota mascota = null;
            string estadoAdopcion = "No adoptado";

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"SELECT id_mascota, nombre, raza, edad, peso, estado_adopcion 
                                 FROM mascotas_adopciones 
                                 WHERE nombre = @Nombre";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            mascota = new Mascota
                            {
                                id_mascota = reader.GetInt32("id_mascota"),
                                nombre = reader.GetString("nombre"),
                                raza = reader.GetString("raza"),
                                edad = reader.GetInt32("edad"),
                                peso = reader.GetDecimal("peso")
                            };

                            estadoAdopcion = reader.GetString("estado_adopcion");
                        }
                    }
                }
            }

            if (mascota == null)
            {
                return NotFound();
            }

            // Verificar si la mascota ya fue adoptada
            if (estadoAdopcion == "Adoptado")
            {
                ViewBag.MensajeAdopcion = "¡Esta mascota ya ha sido adoptada!";
            }
            else
            {
                ViewBag.MensajeAdopcion = null; // No hay mensaje si no está adoptada
            }

            // Usar ViewBag para mostrar el estado de adopción
            ViewBag.EstadoAdopcion = estadoAdopcion;

            return View(mascota);
        }

        [HttpPost]
        public IActionResult AdoptarMascota(string nombre)
        {
            string usuarioCorreo = HttpContext.Session.GetString("Usuario");

            if (string.IsNullOrEmpty(usuarioCorreo))
            {
                return Unauthorized();
            }

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                // Llamar al procedimiento almacenado
                using (MySqlCommand cmd = new MySqlCommand("adoptar_mascota", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@correo_usuario", usuarioCorreo);
                    cmd.Parameters.AddWithValue("@nombre_mascota_adopcion", nombre);
                    cmd.ExecuteNonQuery();
                }

                // Actualizar estado de adopción en la tabla
                string updateQuery = "UPDATE mascotas_adopciones SET estado_adopcion = 'Adoptado' WHERE nombre = @Nombre";
                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@Nombre", nombre);
                    updateCmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Menu", "Menu");
        }
    }
}
