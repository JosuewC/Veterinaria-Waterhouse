using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;

namespace Veterinaria_Waterhouse.Controllers
{
    public class TipoCambioController : Controller
    {
        private readonly HttpClient _httpClient;

        // Constructor donde inicializamos HttpClient
        public TipoCambioController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Acción para mostrar la vista tipo cambio
        public async Task<IActionResult> Tipocambio()
        {
            var apiUrl = "http://localhost:3003/tipo-cambio"; // URL de la API de tipo de cambio

            try
            {
                // Obtener los datos de la API
                var response = await _httpClient.GetStringAsync(apiUrl);
                var tipoCambio = JsonConvert.DeserializeObject<TipoCambioViewModel>(response);

                // Asegúrate de que las propiedades MonedaBase y MonedaDestino estén configuradas
                tipoCambio.MonedaBase = "USD";
                tipoCambio.MonedaDestino = "CRC";

                // Pasar los datos a la vista
                return View(tipoCambio);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                ViewBag.ErrorMessage = "Hubo un error al obtener el tipo de cambio: " + ex.Message;
                return View();
            }
        }
    }

    // Modelo para mapear la respuesta de la API
    public class TipoCambioViewModel
    {
        public double Compra { get; set; }
        public double Venta { get; set; }
        public string FechaActualizacion { get; set; }

        // Agregar las propiedades para MonedaBase y MonedaDestino
        public string MonedaBase { get; set; }  // Para USD
        public string MonedaDestino { get; set; }  // Para CRC
    }
}
