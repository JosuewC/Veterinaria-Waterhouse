using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Veterinaria_Waterhouse.Models;

namespace Veterinaria_Waterhouse.Controllers
{

    public class PagoController : Controller
    {
        public IActionResult Pago()
        {
            return View(); // Retorna la vista sin hacer la llamada a la API aún
        }
        private readonly HttpClient _httpClient;

        public PagoController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Acción para realizar el pago
        [HttpPost]
        public async Task<IActionResult> RealizarPago(Pago pago)
        {
            if (pago == null || string.IsNullOrEmpty(pago.NumeroTarjeta) || string.IsNullOrEmpty(pago.Cvv) || pago.MontoAPagar <= 0)
            {
                return Json(new { success = false, message = "Datos inválidos. Complete todos los campos." });
            }

            try
            {
                // Preparar los datos para el pago
                var requestData = new
                {
                    numero_tarjeta = pago.NumeroTarjeta,
                    cvv = pago.Cvv,
                    monto_a_pagar = pago.MontoAPagar,
                    nombre = pago.Nombre,
                    identificacion = pago.Identificacion,
                    descripcion = pago.Descripcion
                };

                var jsonRequest = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // URL de la API de pago en Node.js
                var apiUrl = "http://localhost:3001/pago"; // Cambia la URL si es necesario

                // Realizar la solicitud POST al endpoint de pago
                var response = await _httpClient.PostAsync(apiUrl, content);

                // Verificar si la solicitud fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Error al realizar el pago. Detalles: " + responseContent });
                }

                var responseData = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(responseData);
                return Json(new { success = true, message = "Pago realizado exitosamente", saldo_anterior = data.saldo_anterior, saldo_actual = data.saldo_actual });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}



