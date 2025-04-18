using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Veterinaria_Waterhouse.Controllers
{
    public class UbiController : Controller
    {
        private readonly string _connectionString = "Server=localhost;Database=veterinaria;User=root;Password=Josue23+WC;";

        // Página principal de Ubicaciones
        public IActionResult Index()
        {
            return View(); // Carga Views/Ubi/Index.cshtml
        }

        // Método para guardar la ubicación seleccionada
        [HttpPost]
        public IActionResult Index(string pais, string provincia, string canton, string distrito, string identificacion)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO ubicaciones_usuario (pais, provincia, canton, distrito, identificacion) " +
                                   "VALUES (@Pais, @Provincia, @Canton, @Distrito, @Identificacion)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Guardamos los NOMBRES de las ubicaciones, no los IDs
                        cmd.Parameters.AddWithValue("@Pais", pais);
                        cmd.Parameters.AddWithValue("@Provincia", provincia);
                        cmd.Parameters.AddWithValue("@Canton", canton);
                        cmd.Parameters.AddWithValue("@Distrito", distrito);
                        cmd.Parameters.AddWithValue("@Identificacion", identificacion);

                        cmd.ExecuteNonQuery();  // Ejecutar la consulta de inserción
                    }
                }

                return Json(new { success = true, message = "Ubicación guardada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar la ubicación: " + ex.Message });
            }
        }

        // Método que devuelve una lista de ubicaciones según el tipo e id_padre
        public JsonResult ObtenerUbicaciones(string tipo, string idPadre)
        {
            List<dynamic> ubicaciones = new List<dynamic>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT id, nombre FROM ubicaciones WHERE tipo = @Tipo";
                if (!string.IsNullOrEmpty(idPadre))
                {
                    query += " AND id_padre = @IdPadre";  // Filtrado por id_padre
                }

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Tipo", tipo);
                    if (!string.IsNullOrEmpty(idPadre))
                    {
                        cmd.Parameters.AddWithValue("@IdPadre", idPadre); // Usamos el idPadre para hacer el filtrado
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ubicaciones.Add(new { id = reader.GetInt32(0), nombre = reader.GetString(1) });
                        }
                    }
                }
            }
            return Json(ubicaciones); // Devuelve las ubicaciones filtradas
        }
    }
}
