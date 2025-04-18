using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace Veterinaria_Waterhouse.Controllers
{
    public class PerfilController : Controller
    {
        private readonly string connectionString = "Server=localhost;Database=veterinaria;User=root;Password=Josue23+WC;";

        public IActionResult perfil()
        {
            var usuarioCorreo = HttpContext.Session.GetString("Usuario");

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand("perfil", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@correo_param", usuarioCorreo);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        // 🔥 Leer el único conjunto de resultados devuelto por el procedimiento almacenado
                        if (reader.Read())
                        {
                            // Datos personales
                            ViewBag.Nombre = reader["nombre"].ToString();
                            ViewBag.Identificacion = reader["identificacion"].ToString();
                            ViewBag.Celular = reader["celular"].ToString();
                            ViewBag.Correo = reader["correo"].ToString();

                            // Mascotas - Verificamos si las columnas existen antes de asignarlas
                            ViewBag.Mascotas = new List<Dictionary<string, string>>();

                            for (int i = 1; i <= 3; i++)
                            {
                                if (reader[$"mascota_{i}"] != DBNull.Value)
                                {
                                    var mascota = new Dictionary<string, string>
                                    {
                                        { "Nombre", reader[$"mascota_{i}"].ToString() },
                                        { "Peso", reader[$"peso_{i}"].ToString() },
                                        { "Edad", reader[$"edad_{i}"].ToString() },
                                        { "Raza", reader[$"raza_{i}"].ToString() }
                                    };

                                    ((List<Dictionary<string, string>>)ViewBag.Mascotas).Add(mascota);
                                }
                            }
                        }
                        else
                        {
                            ViewBag.Error = "No se encontraron datos del usuario.";
                        }
                    }
                }
            }

            return View();
        }
    }
}
