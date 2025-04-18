using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using Veterinaria_Waterhouse.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Veterinaria_Waterhouse.Controllers
{
    public class productos2Controller : Controller
    {
        private readonly string _connectionString = "Server=localhost;Database=veterinaria;User=root;Password=Josue23+WC;";

        public IActionResult productos(string nombre)
        {
            Producto producto = null;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT id_producto, nombre, detalle, unidades, valor_unitario FROM productos_veterinaria WHERE nombre = @Nombre";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            producto = new Producto
                            {
                                IdProducto = reader.GetInt32("id_producto"),
                                NombreProducto = reader.GetString("nombre"),
                                Detalle = reader.GetString("detalle"),
                                Unidades = reader.GetInt32("unidades"),
                                ValorUnitario = reader.GetDecimal("valor_unitario")
                            };
                        }
                    }
                }
            }

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }
    }
}
