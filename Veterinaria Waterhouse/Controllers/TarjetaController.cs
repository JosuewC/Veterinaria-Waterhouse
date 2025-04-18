using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TuProyecto.Controllers
{
    public class TarjetaController : Controller
    {
        private readonly HttpClient _httpClient;

        public TarjetaController()
        {
            _httpClient = new HttpClient();
        }

        // Acción para mostrar el formulario
        public IActionResult RegisterTarjeta()
        {
            return View();
        }

        // Acción para manejar el registro de tarjeta


        [HttpPost]
        public async Task<IActionResult> RegisterTarjeta(string nombre, string identificacion, string numero_tarjeta, string cvv, decimal monto)
        {
            var tarjetaData = new
            {
                nombre,
                identificacion,
                numero_tarjeta,
                cvv,
                monto
            };

            var jsonContent = JsonConvert.SerializeObject(tarjetaData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://localhost:3000/register-tarjeta", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);

                // ✅ Evitar error de serialización
                TempData["Message"] = jsonResponse.message.ToString();

                // Redirigir a MenuController -> Menu
                return RedirectToAction("Menu", "Menu");
            }
            else
            {
                TempData["Message"] = "Hubo un error al registrar la tarjeta.";
                return RedirectToAction("RegisterTarjeta");
            }
        }

    }
}
