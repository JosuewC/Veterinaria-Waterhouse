using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Veterinaria_Waterhouse.Models;
using static Veterinaria_Waterhouse.Models.Medicina;

namespace Veterinaria_Waterhouse.Controllers
{
    public class MedicinaDController : Controller
    {
        private readonly string _connectionString = "Server=localhost;Database=veterinaria;User=root;Password=Josue23+WC;";

        public IActionResult MedicinaD(string nombre_medicina)
        {
            Medicina medicina = null;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT id, nombre_medicina, unidades, valor_unitario, detalle FROM medicinas WHERE nombre_medicina = @NombreMedicina";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@NombreMedicina", nombre_medicina);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            medicina = new Medicina
                            {
                                Id = reader.GetInt32("id"),
                                NombreMedicina = reader.GetString("nombre_medicina"),
                                Unidades = reader.GetInt32("unidades"),
                                ValorUnitario = reader.GetDecimal("valor_unitario"),
                                Detalle = reader.GetString("detalle")
                            };
                        }
                    }
                }
            }

            if (medicina == null)
            {
                return NotFound();
            }

            return View(medicina);
        }
    }
}
