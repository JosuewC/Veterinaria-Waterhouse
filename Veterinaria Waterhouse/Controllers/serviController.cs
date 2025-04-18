using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Veterinaria_Waterhouse.Models;

namespace Veterinaria_Waterhouse.Controllers
{
    public class ServiController : Controller
    {
        private readonly string _connectionString = "Server=localhost;Database=veterinaria;User=root;Password=Josue23+WC;";

        public IActionResult Servi()
        {
            List<servi> servicios = new List<servi>(); // Corrección aquí

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT nombre_servicio, detalle, precio FROM servicios";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            servicios.Add(new servi
                            {
                                nombre_servicio = reader["nombre_servicio"].ToString(),
                                detalle = reader["detalle"].ToString(),
                                precio = Convert.ToDecimal(reader["precio"])
                            });
                        }
                    }
                }
            }

            return View(servicios);
        }
    }
}
