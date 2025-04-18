using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Veterinaria_Waterhouse.Models;

namespace Veterinaria_Waterhouse.Controllers
{
    public class ReservaController : Controller
    {
        private readonly HttpClient _httpClient;

        // Constructor
        public ReservaController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Acción para cargar la vista de reserva
        public IActionResult Reserva()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarReserva(ReservaCita reserva)
        {
            if (ModelState.IsValid)
            {
                // Preparar datos para consumir la API de Node.js
                var requestData = new
                {
                    nombre = reserva.Nombre,
                    id_dueno = reserva.IdDueno,
                    telefono = reserva.Telefono,
                    correo = reserva.Correo,
                    servicio = reserva.Servicio,
                    fecha = reserva.Fecha,
                    precio = reserva.Precio
                };

                var jsonRequest = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // URL de la API que registra la reserva
                var apiUrl = "http://localhost:3000/reserva"; // Verifica que la URL sea correcta
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Reserva registrada con éxito.";
                    return RedirectToAction("Menu", "Menu"); // Redirige al menú después de registrar la reserva
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al registrar la reserva.";
                    return View(reserva); // Muestra el formulario si hubo un error
                }
            }

            return View(reserva); // Si el modelo no es válido, regresa a la vista con los datos
        }
    }
}
