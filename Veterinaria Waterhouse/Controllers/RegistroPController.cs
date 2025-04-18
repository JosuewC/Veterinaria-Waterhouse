using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using static Veterinaria_Waterhouse.Models.RegistroP;

namespace Veterinaria_Waterhouse.Controllers
{
    public class RegistroPController : Controller
    {
        private readonly HttpClient _httpClient;

        // Constructor para inyectar HttpClient
        public RegistroPController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult RegistroP()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarMascota(RegistroMascota mascota)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // URL de la API de Node.js
                    var apiUrl = "http://localhost:3000/register-mascota"; // Cambia esto si es necesario

                    // Crear el objeto con los datos a enviar a la API
                    var requestData = new
                    {
                        NombreDueño = mascota.NombreDueño,
                        id_dueno = mascota.id_dueno,
                        Correo = mascota.Correo,
                        nombre_mascota = mascota.nombre_mascota,
                        Peso = mascota.Peso,
                        Edad = mascota.Edad,
                        Raza = mascota.Raza
                    };

                    // Serializar el objeto a JSON
                    var jsonRequest = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    // Realizar la solicitud POST a la API de Node.js
                    var response = await _httpClient.PostAsync(apiUrl, content);

                    // Verificar la respuesta de la API
                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Mascota registrada con éxito." });
                    }
                    else
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = $"Error al registrar la mascota en la API externa. Detalles: {responseString}" });
                    }
                }
                catch (Exception ex)
                {
                    // Capturar cualquier excepción inesperada
                    return Json(new { success = false, message = $"Error inesperado: {ex.Message}" });
                }
            }
            else
            {
                // Si la validación falla, devuelve un mensaje claro
                return Json(new { success = false, message = "Datos no válidos." });
            }
        }

    }
}
